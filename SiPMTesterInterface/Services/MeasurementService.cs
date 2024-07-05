using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SiPMTesterInterface.AnalysisModels;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;
using SiPMTesterInterface.Hubs;
using SiPMTesterInterface.Interfaces;
using SiPMTesterInterface.Libraries;
using SiPMTesterInterface.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static SiPMTesterInterface.Classes.LEDPulserData;

namespace SiPMTesterInterface.ClientApp.Services
{
    public class MeasurementServiceSettings
    {
        public int BlockCount { get; set; } = 2;
        public int ModuleCount { get; set; } = 2;
        public int ArrayCount { get; set; } = 4;
        public int SiPMCount { get; set; } = 16;

        public MeasurementServiceSettings(IConfiguration config)
        {
            var blockCount = config["MeasurementService:BlockCount"];
            var moduleCount = config["MeasurementService:ModuleCount"];
            var arrayCount = config["MeasurementService:ArrayCount"];
            var sipmCount = config["MeasurementService:SiPMCount"];

            if (blockCount != null)
            {
                int val;
                int.TryParse(blockCount, out val);
                BlockCount = val;
            }
            if (moduleCount != null)
            {
                int val;
                int.TryParse(moduleCount, out val);
                ModuleCount = val;
            }
            if (arrayCount != null)
            {
                int val;
                int.TryParse(arrayCount, out val);
                ArrayCount = val;
            }
            if (sipmCount != null)
            {
                int val;
                int.TryParse(sipmCount, out val);
                SiPMCount = val;
            }
        }
    }

    public class IVMeasurementSettings
    {
        public bool ManualControl { get; set; } = false;
        public double CurrentLimit { get; set; } = 0.01;
        public double CurrentLimitRange { get; set; } = 0.01;
        public int LED { get; set; } = 100;
        public string PulserValueJSONFile { get; set; } = "PulserValues.json";
        public string PulserArrayOffsetJSONFile { get; set; } = "PulserOffsets.json";

        public IVMeasurementSettings(IConfiguration config)
        {
            var manualControl = config["DefaultMeasurementSettings:IV:ManualControl"];
            var currentLimit = config["DefaultMeasurementSettings:IV:CurrentLimit"];
            var currentLimitRange = config["DefaultMeasurementSettings:IV:CurrentLimitRange"];
            var led = config["DefaultMeasurementSettings:IV:LED"];
            var ledValuesJsonFile = config["DefaultMeasurementSettings:IV:PulserValueJSONFile"];
            var arrayOffsetValuesJsonFile = config["DefaultMeasurementSettings:IV:ArrayOffsetsJSONFile"];

            if (manualControl != null)
            {
                bool val;
                bool.TryParse(manualControl, out val);
                ManualControl = val;
            }

            if (currentLimit != null)
            {
                double val;
                double.TryParse(currentLimit, out val);
                CurrentLimit = val;
            }
            if (currentLimitRange != null)
            {
                double val;
                double.TryParse(currentLimitRange, out val);
                CurrentLimitRange = val;
            }
            if (led != null)
            {
                int val;
                int.TryParse(led, out val);
                LED = val;
            }
            if (ledValuesJsonFile != null)
            {
                PulserValueJSONFile = ledValuesJsonFile;
            }
            if (arrayOffsetValuesJsonFile != null)
            {
                PulserValueJSONFile = arrayOffsetValuesJsonFile;
            }
        }
    }

    public class DMMMeasurementSettings
    {
        public double Voltage { get; set; } = 30.0;
        public int Iterations { get; set; } = 5;
        public int CorrectionPercentage { get; set; } = 10;

        public DMMMeasurementSettings(IConfiguration config)
        {
            var voltage = config["DefaultMeasurementSettings:DMMResistance:Voltage"];
            var iterations = config["DefaultMeasurementSettings:DMMResistance:Iterations"];
            var correctionPercentage = config["DefaultMeasurementSettings:DMMResistance:CorrectionPercentage"];

            if (voltage != null)
            {
                double val;
                double.TryParse(voltage, out val);
                Voltage = val;
            }
            if (iterations != null)
            {
                int val;
                int.TryParse(iterations, out val);
                Iterations = val;
            }
            if (correctionPercentage != null)
            {
                int val;
                int.TryParse(correctionPercentage, out val);
                CorrectionPercentage = val;
            }
        }
    }

    public class MeasurementService : MeasurementOrchestrator, IDisposable
    {
        private readonly object _lockObject = new object();

        private bool disposed = false;

        //connected instruments
        private NIMachine? niMachine;
        private PSoCCommunicator? Pulser;
        private HVPSU? hvPSU;

        private bool MeasurementStopped = false;

        private TaskTypes _currentTask = TaskTypes.Idle;

        public MeasurementServiceSettings MeasurementServiceSettings { get; private set; }
        public IVMeasurementSettings ivMeasurementSettings { get; private set; }
        public DMMMeasurementSettings dmmMeasurementSettings { get; private set; }

        private DeviceStatesModel DeviceStates;
        public LEDPulserData PulserValues;
        private ServiceStateHandler serviceState;
        private CoolerStateHandler coolerState;
        private List<CurrentSiPMModel> ivSiPMs;
        private List<CurrentSiPMModel> spsSiPMs;

        private int actualBlock = -1;
        private int actualModule = -1;
        private bool waitForTemperatureStabilisation = false;
        private bool currentlyWaitingForTemperatureStabilisation = false;

        private readonly IConfiguration Configuration;

        private readonly IHubContext<UpdatesHub, IStateContext> _hubContext;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MeasurementService> _logger;

        public long StartTimestamp { get; private set; } = 0;
        public long EndTimestamp { get; private set; } = 0;

        public TaskTypes CurrentTask
        {
            get
            {
                return _currentTask;
            }
            set
            {
                _currentTask = value;
                _logger.LogInformation($"Current task set to {_currentTask}");
                _hubContext.Clients.All.ReceiveCurrentTask(_currentTask);
            }
        }

        private DateTime utcDate;

        private void PopulateServiceState()
        {
            if (!DeviceStates.IsAllEnabledDeviceStarted() && false)
            {
                CreateAndSendLogMessage("PopulateServiceState", "MeasurementService has devices which are not started", LogMessageType.Error, Devices.Unknown, false, ResponseButtons.OK, MeasurementType.Unknown);
                throw new ApplicationException("MeasurementService has devices which are not started");
            }
            serviceState = new ServiceStateHandler(MeasurementServiceSettings.BlockCount, MeasurementServiceSettings.ModuleCount,
                                                        MeasurementServiceSettings.ArrayCount, MeasurementServiceSettings.SiPMCount);
            utcDate = DateTime.UtcNow;
            // Flatten the structure and save all details
            var SiPMs = globalState.CurrentRun.Blocks
                .SelectMany((block, blockIdx) => block.Modules
                    .SelectMany((module, moduleIdx) => module.Arrays
                        .SelectMany((array, arrayIdx) => array.SiPMs
                            .Select((sipm, sipmIdx) => new
                            {
                                Barcode = array.Barcode,
                                BlockIndex = blockIdx,
                                ModuleIndex = moduleIdx,
                                ArrayIndex = arrayIdx,
                                SiPMIndex = sipmIdx,
                                SiPM = sipm
                            })
                        )
                    )
                )
                //.Where(item => item.SiPM.IV == 1)
                .ToList();

            foreach (var item in SiPMs)
            {
                CurrentMeasurementDataModel c = serviceState.GetSiPMMeasurementData(item.BlockIndex, item.ModuleIndex, item.ArrayIndex, item.SiPMIndex);
                c.SiPMLocation = new CurrentSiPMModel(item.BlockIndex, item.ModuleIndex, item.ArrayIndex, item.SiPMIndex);
                c.Barcode = item.Barcode;
                c.SiPMMeasurementDetails = item.SiPM;
                //use global IV list if IV is enabled but voltage list is not set directly for this sipm
                if (c.SiPMMeasurementDetails.IVVoltages.Count <= 0 && c.SiPMMeasurementDetails.IV != 0)
                {
                    c.SiPMMeasurementDetails.IVVoltages = globalState.CurrentRun.IVVoltages;
                }
                //same for SPS
                if (c.SiPMMeasurementDetails.SPSVoltages.Count <= 0 && c.SiPMMeasurementDetails.SPS != 0)
                {
                    c.SiPMMeasurementDetails.SPSVoltages = globalState.CurrentRun.SPSVoltages;
                }
            }
        }

        public LogMessageModel CreateAndSendLogMessage(string sender, string message, LogMessageType logType, Devices device, bool interactionNeeded, ResponseButtons buttons, MeasurementType type = MeasurementType.Unknown)
        {
            LogMessageModel logMessage;
            logMessage = new LogMessageModel(sender, message, logType, device, type, interactionNeeded, buttons);
            _hubContext.Clients.All.ReceiveLogMessage(logMessage);
            Logs.Add(logMessage);
            if (logType == LogMessageType.Error)
            {
                CurrentTask = TaskTypes.Waiting;
            }
            return logMessage;
        }

        public void CheckAndRunNextAsync()
        {
            Task t = Task.Run(() =>
            {
                try
                {
                    CheckAndRunNext();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{ex.Message}");
                }
            });
        }

        public void CheckBlockStatus(int previousBlock, int nextBlock)
        {
            //if (actualBlock != sipms[0].Block && actualBlock > -1 && coolerState.GetAPSUState(actualBlock))
            if (previousBlock != nextBlock && previousBlock > -1)
            {
                double voltage;
                double current;
                SetPSU(previousBlock, PSUs.PSU_A, false);
                SetPSU(nextBlock, PSUs.PSU_A, true);
            }
        }

        public void CheckAndRunNext()
        {
            MeasurementType Type;
            object nextMeasurementData;
            List<CurrentSiPMModel> sipms;

            //turn off pulser, disconnect all relays
            EndTimestamp = TimestampHelper.GetUTCTimestamp(); ; //just to make sure something is saved

            if (MeasurementStopped)
            {
                CurrentTask = TaskTypes.Idle;
                _logger.LogWarning($"Measurement stopped");
                return;
            }

            var logList = GetAttentionNeededLogs();

            if (logList.Count > 0)
            {
                _logger.LogWarning($"{logList.Count} logs waiting for user response");
                return;
            }
            int nextBlock;
            int nextModule;
            MeasurementType nextMeasurementType;
            try
            {
                
                if (IsBlockOrModuleChanging(out nextBlock, out nextModule, out nextMeasurementType))
                {
                    _logger.LogDebug($"Next measurement type is {nextMeasurementType}");
                    if (!coolerState.GetCoolerSettings(nextBlock, nextModule).Enabled)
                    {
                        // enable next
                        CoolerSettingsModel s = new CoolerSettingsModel();
                        s.Block = nextBlock;
                        s.Module = nextModule;
                        s.Enabled = true;
                        s.TargetTemperature = 24.0;
                        s.FanSpeed = 10;
                        SetCooler(s);

                        // disable previous
                        if (actualBlock > -1 && actualModule > -1)
                        {
                            s.Block = actualBlock;
                            s.Module = actualModule;
                            s.Enabled = false;
                            s.TargetTemperature = 24.0;
                            s.FanSpeed = 10;
                            SetCooler(s);
                        }
                    }
                    actualBlock = nextBlock;
                    actualModule = nextModule;
                }

                if (nextMeasurementType == MeasurementType.IVMeasurement)
                {
                    _logger.LogDebug($"Let's wait for temperature stabilisation");
                    waitForTemperatureStabilisation = true;
                }
                else
                {
                    _logger.LogDebug($"Do not wait for temperature stabilisation");
                    waitForTemperatureStabilisation = false;
                }
            }
            catch (SerialTimeoutLimitReachedException ex)
            {
                CreateAndSendLogMessage("Measurement Service - Check and Run next - Pulser - Init", ex.Message + " - Do you want to reinitialize Pulser?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                CreateAndSendLogMessage("Measurement Service - Check and Run next - APSU", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.APSU, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
                return;
            }


            //wait for temperature stabilisation
            
            if (nextMeasurementType == MeasurementType.IVMeasurement)
            {
                _logger.LogDebug($"WaitForTemperatureStabilisation={waitForTemperatureStabilisation}, Block={actualBlock}, Module={actualModule}, TempStable={coolerState.GetCoolerSettings(actualBlock, actualModule).State.IsTemperatureStable}");
                if (waitForTemperatureStabilisation && !coolerState.GetCoolerSettings(actualBlock, actualModule).State.IsTemperatureStable)
                {
                    currentlyWaitingForTemperatureStabilisation = true;
                    _logger.LogDebug($"Waiting for temperature stabilisation...");
                    CurrentTask = TaskTypes.TemperatureStabilisation;
                    return;
                }
                else
                {
                    _logger.LogDebug($"Temperature stabilised");
                    currentlyWaitingForTemperatureStabilisation = false;
                }
            }
            

            Console.WriteLine("Checking new iteration...");
            if (GetNextIterationData(out Type, out nextMeasurementData, out sipms))
            {
                Console.WriteLine($"Next measurement type is {Type}");
                if (Type == MeasurementType.DMMResistanceMeasurement)
                {
                    CurrentTask = TaskTypes.DMMResistance;
                    NIDMMStartModel niDMMStart = nextMeasurementData as NIDMMStartModel;
                    niDMMStart.DMMResistance = globalState.CurrentRun.DMMResistance;
                    try
                    {
                        Pulser.SetMode(0, 0, 0, 0, MeasurementMode.MeasurementModes.DMMResistanceMeasurement, new[] { 0, 0, 0, 0 });
                    }
                    catch (SerialTimeoutLimitReachedException ex)
                    {
                        CreateAndSendLogMessage("Measurement Service - Check and Run next - Pulser - Init", ex.Message + " - Do you want to reinitialize Pulser?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                        _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error on DMM CheckAndRunNext(): {ex.Message}");
                        globalState.GlobalIVMeasurementState = MeasurementState.Error;
                        CreateAndSendLogMessage("CheckAndRunNext - DMM Resistance", ex.Message, LogMessageType.Error, Devices.Pulser, true, ResponseButtons.StopRetryContinue, MeasurementType.DMMResistanceMeasurement);
                        return;
                    }
                    niMachine?.StartDMMResistanceMeasurement(niDMMStart);
                }

                else if (Type == MeasurementType.IVMeasurement)
                {
                    CurrentTask = TaskTypes.IV;
                    ivSiPMs = sipms;

                    //if next measurement is using another block
                    

                    if (Pulser.Enabled && !coolerState.GetAPSUState(actualBlock)) //if not enabled
                    {
                        try
                        {
                            double voltage = 0.0;
                            double current = 0.0;
                            //Pulser.EnablePSU(actualBlock, PSUs.PSU_A, out voltage, out current);
                            SetPSU(actualBlock, PSUs.PSU_A, true);
                            CreateAndSendLogMessage("Measurement Service - APSU", $"PSU A for Block {actualBlock} enabled. Voltage: {voltage}, Current: {current}", LogMessageType.Info, Devices.APSU, false, ResponseButtons.OK, MeasurementType.Unknown);
                        }
                        catch (SerialTimeoutLimitReachedException ex)
                        {
                            CreateAndSendLogMessage("Measurement Service - Check and Run next - Pulser - Init", ex.Message + " - Do you want to reinitialize Pulser?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                            _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
                            return;
                        }
                        catch (Exception ex)
                        {
                            CreateAndSendLogMessage("Measurement Service - Check and Run next - APSU", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.APSU, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                            _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
                            return;
                        }
                    }

                    actualBlock = sipms[0].Block;
                    actualModule = sipms[0].Module;

                    NIIVStartModel niIVStart = nextMeasurementData as NIIVStartModel; //some settings duplicated here and in ServiceState
                    if (sipms.Count != 1)
                    {
                        CreateAndSendLogMessage("CheckAndRunNext - IV", "Can not measure more than one SiPM at a time for IV", LogMessageType.Error, Devices.NIMachine, false, ResponseButtons.OK, MeasurementType.IVMeasurement);
                        _logger.LogError("Can not measure more than one SiPM at a time for IV");
                        return;
                    }
                    //TODO: Get the right led pulser values here
                    //set IV settings
                    //change SiPM relays
                    int[] pulserLEDValues = new[]
                    {
                        //LEDValueHelper.GetPulserValueForSiPM(ivSiPMs[0], PulserValues).PulserValue,
                        PulserValues.GetPulserValue(ivSiPMs[0]),
                        0,
                        0,
                        0,
                    };
                    try
                    {
                        Pulser?.SetMode(ivSiPMs[0].Block, ivSiPMs[0].Module, ivSiPMs[0].Array, ivSiPMs[0].SiPM, MeasurementMode.MeasurementModes.IV,
                            new[] { pulserLEDValues[0], pulserLEDValues[1], pulserLEDValues[2], pulserLEDValues[3] });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error on IV CheckAndRunNext(): {ex.Message}");
                        globalState.GlobalIVMeasurementState = MeasurementState.Error;
                        CreateAndSendLogMessage("CheckAndRunNext - IV", ex.Message, LogMessageType.Error, Devices.Pulser, true, ResponseButtons.StopRetryContinue, MeasurementType.IVMeasurement);
                        return;
                    }
                    CurrentMeasurementDataModel c = serviceState.GetSiPMMeasurementData(ivSiPMs[0].Block, ivSiPMs[0].Module, ivSiPMs[0].Array, ivSiPMs[0].SiPM);
                    c.IVMeasurementID = niIVStart.Identifier;
                    niMachine?.StartIVMeasurement(niIVStart);
                }

                //WIP
                else if (Type == MeasurementType.SPSMeasurement)
                {
                    CurrentTask = TaskTypes.SPS;
                    /* SPS - 0. element pulser
                     * DualSPS - 0, 1. element pulser
                     * Invalid
                     * QuadSPS - 0, 1, 2, 3 element pulser
                     */

                    /* Up/Down selection Quad:
                     * Module 0, Array 0 - 0,2
                     * Module 0, Array 1 - 1,3
                     * 
                     * Up/Down selection Dual:
                     * Module 0, Array 0 - 0,2
                     * Module 0, Array 1 - 1,3
                     * OR
                     * Module 1, Array 0 - 0,2
                     * Module 1, Array 1 - 1,3
                     * 
                     * Up/Down selection Single:
                     * Module x, Array y, SiPM z - X Y Z
                     * Intensity array first item must be set all the time
                     */

                    /*
                     * Pulser connects the HV to the right SiPMs set above
                     */

                    /*
                     * DAQ flow chart:
                     * Off -> SPS V measure -> Set HVs -> SPS V measure for all channels -> Desired SPS mode -> Measurement
                     * (Next HV level -> Set HVs -> SPS V measure for all channels -> Measurement) x N V level
                     * HVs off -> Off
                     */
                    spsSiPMs = sipms;
                    int[] sipmCounts = GetSiPMCountPerBlock(spsSiPMs);

                    for (int i = 0; i < sipmCounts.Count(); i++)
                    {
                        MeasurementMode.MeasurementModes mode = MeasurementMode.MeasurementModes.Off;
                        switch(sipmCounts[i])
                        {
                            case 0:
                                mode = MeasurementMode.MeasurementModes.Off;
                                break;
                            case 1:
                                mode = MeasurementMode.MeasurementModes.SPS;
                                break;
                            case 2:
                                mode = MeasurementMode.MeasurementModes.DualSPS;
                                break;
                            //can not do this!
                            case 3:
                                mode = MeasurementMode.MeasurementModes.QuadSPS;
                                break;
                            case 4:
                                mode = MeasurementMode.MeasurementModes.QuadSPS;
                                break;
                        }
                    }
                    
                }
            }
            else //end of measurement
            {
                TryEndMeasurement();
            }
        }

        public void TryEndMeasurement()
        {
            try
            {
                if (Pulser != null && Pulser.Enabled)
                {
                    foreach (var activeBlock in Pulser.ActiveBlocks)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            CoolerSettingsModel s = coolerState.GetCopyOfCoolerSettings(activeBlock, i);
                            if (s.Enabled)
                            {
                                s.Enabled = false;
                                SetCooler(s);
                            }
                        }

                        SetPSU(activeBlock, PSUs.PSU_A, false);
                        //CreateAndSendLogMessage("Measurement Service - TryEndMeasurement", $"PSU A disabled for block {activeBlock}.", LogMessageType.Info, Devices.Pulser, false, ResponseButtons.OK, MeasurementType.Unknown);
                    }
                    
                }
                actualBlock = -1;
                actualModule = -1;
                CurrentTask = TaskTypes.Finished;
                EndTimestamp = TimestampHelper.GetUTCTimestamp();
            }
            catch (SerialTimeoutLimitReachedException ex)
            {
                CreateAndSendLogMessage("Measurement Service - Try end measurement - Pulser - Init", ex.Message + " - Do you want to reinitialize Pulser?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                CreateAndSendLogMessage("Measurement Service - TryEndMeasurement - APSU", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.APSU, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                _logger.LogError($"Measurement service error: {ex.Message}");
            }
        }

        public static int[] GetSiPMCountPerBlock(List<CurrentSiPMModel> selectedSiPMs)
        {
            int[] sipmCounts;
            var siPMCountPerBlock = selectedSiPMs
            .GroupBy(sipm => sipm.Block)
            .Select(group => new
            {
                Block = group.Key,
                SiPMCount = group.Count()
            })
            .ToList();

            sipmCounts = new int[siPMCountPerBlock.Count];

            for(int i = 0; i < siPMCountPerBlock.Count; i++)
            {
                sipmCounts[i] = siPMCountPerBlock[i].SiPMCount;
                Console.WriteLine($"Block: {siPMCountPerBlock[i].Block}, SiPMCount: {siPMCountPerBlock[i].SiPMCount}");
            }
            return sipmCounts;
        }

        private Task RunAnalysis(CurrentMeasurementDataModel data)
        {
            Task t = Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Starting analysis task...");
                    for (int i = 0; i < data.IVResult.SMUCurrent.Count; i++)
                    {
                        if (data.IVResult.SMUCurrent[i] > 10E-6)
                        {
                            data.IVResult.AnalysationResult.IsCurrentCheckOK = true;
                        }
                    }
                    data.IVResult.AnalysationResult.Analysed = true;

                    if (data.IVResult.AnalysationResult.IsCurrentCheckOK)
                    {
                        string outputPath = Path.Combine(FilePathHelper.GetCurrentDirectory(), utcDate.ToString("yyyyMMddHHmmss"), data.Barcode, "IVAnalysisResult");
                        AnalysisProperties analysisProperties = new AnalysisProperties();
                        analysisProperties.nDerivativeSmooth = 0;
                        analysisProperties.nlnSmooth = 0;
                        analysisProperties.nPreSmooth = 0;
                        analysisProperties.fitWidth = 150; //divided by 1000 in analysis library
                        RootIVAnalyser.Analyse(data, outputPath, null);
                    }
                }
                catch (DllNotFoundException ex)
                {
                    _logger.LogError($"{ex.Message}");
                }
                catch (Exception ex)
                {
                    CreateAndSendLogMessage("RunAnalysis", ex.Message, LogMessageType.Error, Devices.Unknown, false, ResponseButtons.OK, MeasurementType.IVMeasurement);
                    _logger.LogError($"{ex.Message}");
                }

                try
                {
                    FileOperationHelper.SaveIVResult(data, utcDate.ToString("yyyyMMddHHmmss"));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error while writing JSON data: {ex.Message}");
                }

                try
                {
                    double[] temps;
                    if (data.IVResult.Temperatures.Count > 0)
                    {
                        int index = (int)data.IVResult.Temperatures.Count / 2;
                        temps = data.IVResult.Temperatures[index].Module1;
                    }
                    else
                    {
                        temps = new double[8];
                        for (int i = 0; i < 8; i++)
                        {
                            temps[i] = 25.0;
                        }
                    }

                    FileOperationHelper.WriteBinaryIVData(data.SiPMLocation.Module, data.SiPMLocation.SiPM, "IVBinary", temps, data.IVResult.DMMVoltage.ToArray(), data.IVResult.SMUVoltage.ToArray(), data.IVResult.SMUCurrent.ToArray(), data.DMMResistanceResult.Resistance, data.Barcode, data.IVResult.StartTimestamp);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error while writing binary data: {ex.Message}");
                }

                _hubContext.Clients.All.ReceiveIVAnalysationResult(data.SiPMLocation, new IVMeasurementHubUpdate(data.IVResult.AnalysationResult, new IVTimes(data.IVResult.StartTimestamp, data.IVResult.EndTimestamp))); //send mesaurement update
                _logger.LogInformation("Analysis task done");

            });
            return t;
        }

        public void StartMeasurement(MeasurementStartModel measurementData)
        {
            PrepareMeasurement(measurementData);
            PopulateServiceState(); //store measurements
            CreateAndSendLogMessage("StartMeasurement", "Measurement is starting...", LogMessageType.Info, Devices.Unknown, false, ResponseButtons.OK, MeasurementType.Unknown);
            MeasurementStopped = false;
            CheckAndRunNextAsync();
            StartTimestamp = TimestampHelper.GetUTCTimestamp();
        }

        public void StopMeasurement()
        {

            MeasurementStopped = true;
            CheckAndRunNext();
            niMachine?.StopMeasurement();
            Pulser?.DisablePSU(0, PSUs.PSU_A);
        }

        private void InitNI()
        {
            try
            {
                var niLogger = _loggerFactory.CreateLogger<NIMachine>();
                if (niMachine == null) niMachine = new NIMachine(Configuration, niLogger);
                niMachine.OnConnectionStateChanged += OnIVConnectionStateChangeCallback;
                niMachine.OnMeasurementStateChanged += OnIVMeasurementStateChangeCallback;

                niMachine.OnIVMeasurementDataReceived += OnIVMeasurementDataReceived;
                niMachine.OnDMMMeasurementDataReceived += OnDMMMeasurementDataReceived;

                if (niMachine.Enabled)
                {
                    DeviceStates.SetEnabled(Devices.NIMachine, true);
                    niMachine.Init();
                    DeviceStates.SetInitState(Devices.NIMachine, true);
                    niMachine.Start();
                    DeviceStates.SetStartedState(Devices.NIMachine, true);
                }
                else
                {
                    DeviceStates.SetEnabled(Devices.NIMachine, false);
                }
            }
            catch (Exception ex)
            {
                CreateAndSendLogMessage("Measurement Service - Init - NI Machine", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.NIMachine, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
            }
        }

        private void RefreshAPSUStates()
        {
            try
            {
                APSUDiagResponse diagResp = Pulser.GetAPSUStates();
                for (int i = 0; i < coolerState.BlockNum; i++)
                {
                    coolerState.SetAPSUState(i, diagResp.GetAPSUState(i));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error while updating APSU states: {ex.Message}");
            }
        }

        private void InitPulser()
        {
            try
            {
                if (Pulser != null)
                {
                    Pulser.Stop();
                    Pulser.Close();
                    Pulser = null;
                }

                var psocSerialLogger = _loggerFactory.CreateLogger<SerialPortHandler>();
                SerialSettings pulserSettings = new SerialSettings(Configuration, "Pulser");

                if (pulserSettings.Enabled && pulserSettings.AutoDetect)
                {
                    string port = SerialPortHandler.GetAutoDetectedPort(psocSerialLogger, pulserSettings.BaudRate, 500, pulserSettings.AutoDetectString, pulserSettings.AutoDetectExpectedAnswer);
                    pulserSettings.SerialPort = port;
                }

                if (Pulser == null) Pulser = new PSoCCommunicator(pulserSettings, psocSerialLogger);
                Pulser.OnSerialStateChanged += Pulser_OnSerialStateChanged;
                Pulser.OnDataReadout += Pulser_OnDataReadout;
                Pulser.OnSerialErrorMessageReceived += Pulser_OnSerialErrorMessageReceived;

                double voltage = 0.0;
                double current = 0.0;

                if (Pulser.Enabled)
                {
                    DeviceStates.SetEnabled(Devices.Pulser, true);
                    Pulser.Init();
                    DeviceStates.SetInitState(Devices.Pulser, true);
                    Pulser.Start();
                    //Pulser.EnablePSU(0, PSUs.PSU_A, out voltage, out current);
                    //CreateAndSendLogMessage("Measurement Service - Init - Pulser", $"PSU A enabled. Voltage: {voltage}, Current: {current}", LogMessageType.Info, Devices.Pulser, false, ResponseButtons.OK, MeasurementType.Unknown);
                    DeviceStates.SetStartedState(Devices.Pulser, true);

                    RefreshAPSUStates();
                }
                else
                {
                    DeviceStates.SetEnabled(Devices.Pulser, false);
                }
            }
            catch (Exception ex)
            {
                CreateAndSendLogMessage("Measurement Service - Init - Pulser", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
            }
        }

        private void Pulser_OnSerialErrorMessageReceived(object? sender, SerialErrorMessageReceivedEventArgs e)
        {
            /*
             * 
             */
            if (!e.IsError)
            {
                CreateAndSendLogMessage("Measurement Service - Pulser", e.ReceivedMessage, LogMessageType.Error, Devices.Pulser, true, ResponseButtons.StopRetryContinue, MeasurementType.Unknown);
                _logger.LogError($"Invalid state of Pulser: {e.ReceivedMessage}");
                return;
            }
            if (e.ReceivedMessage.Contains("A_PSU_disabled!!", StringComparison.InvariantCultureIgnoreCase))
            {
                CreateAndSendLogMessage("Measurement Service - Pulser", "A_PSU is disabled. Trying to enable...", LogMessageType.Info, Devices.Pulser, false, ResponseButtons.OK, MeasurementType.Unknown);
                _logger.LogError($"A_PSU is disabled. Trying to enable...");
                if (CurrentTask == TaskTypes.IV)
                {
                    SetRetryFailedMeasurement(MeasurementType.IVMeasurement);
                }
                CheckAndRunNextAsync();
            }

            else
            {
                CreateAndSendLogMessage("Measurement Service - Pulser", $"{e.ReceivedMessage}", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.StopRetryContinue, MeasurementType.Unknown);
                _logger.LogError($"A_PSU is disabled. Trying to enable...");
            }
        }

        private void InitHVPSU()
        {
            try
            {
                var hvpsuSerialLogger = _loggerFactory.CreateLogger<SerialPortHandler>();
                if (hvPSU == null) hvPSU = new HVPSU(Configuration, hvpsuSerialLogger);

                if (hvPSU.Enabled)
                {
                    DeviceStates.SetEnabled(Devices.HVPSU, true);
                    hvPSU.Init();
                    DeviceStates.SetInitState(Devices.HVPSU, true);
                    hvPSU.Start();
                    DeviceStates.SetStartedState(Devices.HVPSU, true);
                }
                else
                {
                    DeviceStates.SetEnabled(Devices.HVPSU, false);
                }
            }
            catch (SerialTimeoutLimitReachedException ex)
            {
                CreateAndSendLogMessage("Measurement Service - Init - HVPSU", ex.Message + " - Do you want to reinitialize HVPSU?", LogMessageType.Error, Devices.HVPSU, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                CreateAndSendLogMessage("Measurement Service - Init - HVPSU", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.HVPSU, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
            }
        }

        public void InitDevices()
        {
            InitNI();
            InitPulser();
            InitHVPSU();
        }

        public MeasurementService(ILoggerFactory loggerFactory, IHubContext<UpdatesHub, IStateContext> hubContext, IConfiguration configuration) : base(loggerFactory.CreateLogger<MeasurementOrchestrator>())
        {
            Configuration = configuration;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<MeasurementService>();
            _hubContext = hubContext;

            NIMachineSettings niSettings;
            SerialSettings pulserSettings;
            SerialSettings hvpsuSeettings;

            try
            {
                MeasurementServiceSettings = new MeasurementServiceSettings(Configuration);
                ivMeasurementSettings = new IVMeasurementSettings(Configuration);
                dmmMeasurementSettings = new DMMMeasurementSettings(Configuration);

                niSettings = new NIMachineSettings(configuration);
                pulserSettings = new SerialSettings(configuration, "Pulser");
                hvpsuSeettings = new SerialSettings(configuration, "HVPSU");
            }
            catch (Exception ex)
            {
                CreateAndSendLogMessage("Measurement Service - Settings", ex.Message, LogMessageType.Error, Devices.Unknown, false, ResponseButtons.OK, MeasurementType.Unknown);
                _logger.LogError($"Measurement servicse is unavailable because of an error: {ex.Message}");
            }

            try
            {
                if (MeasurementServiceSettings != null && ivMeasurementSettings != null)
                {
                    PulserValues = new LEDPulserData();
                    PulserValues.Init(MeasurementServiceSettings.BlockCount, MeasurementServiceSettings.ModuleCount,
                                    MeasurementServiceSettings.ArrayCount, MeasurementServiceSettings.SiPMCount, ivMeasurementSettings.LED);

                    PulserValues.ApplyUpdatesFromJson(ivMeasurementSettings.PulserValueJSONFile);
                    PulserValues.ApplyLEDPulserOffsetUpdatesFromJson(ivMeasurementSettings.PulserArrayOffsetJSONFile);

                        //= LEDValueHelper.GenerateDefaultIVPulserData(MeasurementServiceSettings.BlockCount, MeasurementServiceSettings.ModuleCount,
                        //                    MeasurementServiceSettings.ArrayCount, MeasurementServiceSettings.SiPMCount, ivMeasurementSettings.LED);
                        //LEDValueHelper.ReadAndUpdateFromJsonFile(ivMeasurementSettings.PulserValueJSONFile, PulserValues);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Can not update pulser values: {ex.Message}");
            }

            DeviceStates = new DeviceStatesModel();

            coolerState = new CoolerStateHandler(MeasurementServiceSettings.BlockCount, MeasurementServiceSettings.ModuleCount);
            coolerState.OnCoolerTemperatureStabilizationChanged += CoolerState_OnCoolerTemperatureStabilizationChanged;

            MeasurementStartModel startModel = new MeasurementStartModel();
            startModel.MeasureDMMResistance = true;
            startModel.DMMResistance.Voltage = 30.0;
            startModel.DMMResistance.Iterations = 3;
            startModel.DMMResistance.CorrectionPercentage = 10;

            startModel.IV = 1;
            startModel.SPS = 0;
            startModel.SPSVoltagesIsOffsets = 0;
            startModel.IVVoltages.Add(10.0);
            startModel.IVVoltages.Add(11.0);
            startModel.IVVoltages.Add(12.0);
            startModel.IVVoltages.Add(13.0);

            startModel.SPSVoltages.Add(8.0);
            startModel.SPSVoltages.Add(9.0);
            startModel.SPSVoltages.Add(10.0);
            startModel.SPSVoltages.Add(11.0);

            startModel.Blocks = new List<Block>();

            for (int i = 0; i < 2; i++)
            {
                Block block = new Block();
                for (int j = 0; j < 2; j++)
                {
                    Module module = new Module();
                    for (int k = 0; k < 4; k++)
                    {
                        Models.Array array = new Models.Array();
                        for (int l = 0; l < 16; l++)
                        {
                            SiPM sipm = new SiPM();
                            sipm.IV = 1;
                            sipm.SPS = 1;
                            array.SiPMs.Add(sipm);
                        }
                        module.Arrays.Add(array);
                    }
                    block.Modules.Add(module);
                }
                startModel.Blocks.Add(block);
            }

            Console.WriteLine($"TestQuery: {JsonConvert.SerializeObject(startModel)}");
        }

        private void CoolerState_OnCoolerTemperatureStabilizationChanged(object? sender, CoolerTemperatureStabilizationChangedEventArgs e)
        {
            _logger.LogDebug($"Temperature state changed for Block {e.Block}, Module {e.Module} to {e.CurrentState} from {e.PreviousState}");
            if (e.Block == actualBlock && e.Module == actualModule && e.CurrentState && currentlyWaitingForTemperatureStabilisation)
            {
                _logger.LogDebug("Checking temperature continue conditions");
                CheckAndRunNextAsync();
            }
        }

        //Events--------------------------------------------------------------------

        private void OnIVConnectionStateChangeCallback(object? sender, ConnectionStateChangedEventArgs e)
        {
            globalState.IVModel.ConnectionState = e.State;
            //Send updates to the connected clients: previous state, current state
            _hubContext.Clients.All.ReceiveIVConnectionStateChange(e.State);
        }

        private void OnIVMeasurementStateChangeCallback(object? sender, MeasurementStateChangedEventArgs e)
        {
            /*
             * Save the global state. If there is an available iteration but the measurement is not running
             * it means that the current iteration is finished, but the full set is not measured yet.
             */
                if (e.State != MeasurementState.Running && !IsIVIterationAvailable())
            {
                globalState.GlobalIVMeasurementState = e.State; //that can be Finished, Not running or Unknown
                _hubContext.Clients.All.ReceiveGlobalIVMeasurementStateChange(globalState.GlobalIVMeasurementState);
            }
            globalState.IVModel.MeasurementState = e.State;
            Console.WriteLine($"IV state changed from {e.Previous} to {e.State}");
            //Send updates to the connected clients: Current global state, state, current sipm, number of measurements, current measurement number
            _hubContext.Clients.All.ReceiveIVMeasurementStateChange(globalState.IVModel.MeasurementState);

            /*
             * Next measurement will start when data is received from instrument
            if (e.State == MeasurementState.Finished && IsIVIterationAvailable())
            {
                
            }
            */
        }

        private void OnDMMMeasurementDataReceived(object? sender, DMMMeasurementDataReceivedEventArgs e)
        {
            _logger.LogInformation("DMMMeasurementReceived");
            CheckAndRunNext();
            //save data here
            serviceState.AppendDMMResistanceMeasurement(e.Data);
            try
            {
                string path = Path.Combine(FilePathHelper.GetCurrentDirectory(), "DMMResult");
                FileOperationHelper.CreateOrAppendToFileDMMMeasurement(path, "DMMResistance.json", e.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while adding DMM resistance data to file ({ex.Message})");
            }
        }

        private void OnIVMeasurementDataReceived(object? sender, IVMeasurementDataReceivedEventArgs e)
        {
            //save data here

            _logger.LogInformation("IVMeasurementReceived");
            CurrentMeasurementDataModel c;
            if (serviceState.GetSiPMIVMeasurementData(e.Data.Identifier, out c))
            {
                c.IVResult = e.Data;
                c.IVResult.AnalysationResult = new IVAnalysationResult();
                c.IsIVDone = true;
            }
            c.IVResult.Temperatures = Temperatures.Where(item => item.Timestamp >= c.IVResult.StartTimestamp && item.Timestamp <= c.IVResult.EndTimestamp).ToList();

            //append latest dmm resistance measurement if available
            c.DMMResistanceResult = serviceState.DMMResistances.LastOrDefault(new DMMResistanceMeasurementResponseModel());
            

            if (c.IVResult.ErrorHappened)
            {
                CreateAndSendLogMessage("Failed IV measurement", $"IV measurement for {c.SiPMLocation.Block}, {c.SiPMLocation.Module}, {c.SiPMLocation.Array}, {c.SiPMLocation.SiPM} is failed. Reason: {c.IVResult.ErrorMessage}. Choose the next step!", LogMessageType.Error, Devices.NIMachine, true, ResponseButtons.StopRetryContinue, MeasurementType.IVMeasurement);
                return;
            }

            //Run analysis and save data there even if analysis fails
            RunAnalysis(c); //async call

            _hubContext.Clients.All.ReceiveSiPMIVMeasurementDataUpdate(c.SiPMLocation);

            CheckAndRunNext();
        }

        private void Pulser_OnDataReadout(object? sender, PSoCCommuicatorDataReadEventArgs e)
        {
            ModuleCoolerState m0 = e.Data.CoolerState.GetModuleState(0);
            ModuleCoolerState m1 = e.Data.CoolerState.GetModuleState(1);
            coolerState.SetModuleCoolerState(m0);
            coolerState.SetModuleCoolerState(m1);
            coolerState.SetModuleTemperatures(e.Data.Temperature);
            _hubContext.Clients.All.ReceivePulserTempCoolerData(e);
        }

        private void Pulser_OnSerialStateChanged(object? sender, SerialConnectionStateChangedEventArgs e)
        {
            _hubContext.Clients.All.ReceivePulserStateChange(e);
        }

        //Getter-Setter--------------------------------------------------------------------
        public List<LogMessageModel> GetUnresolvedLogs()
        {
            var list = Logs.Where(e => e.Resolved != true).ToList();
            return list;
        }

        public List<LogMessageModel> GetAttentionNeededLogs()
        {
            var list = Logs.Where(e => e.Resolved != true && e.NeedsInteraction).ToList();
            return list;
        }

        public List<LogMessageModel> GetAllLogs()
        {
            return Logs;
        }

        public bool CheckResponseButtonIsValid(ResponseButtons availableButtons, ResponseButtons userResponse)
        {
            bool retVal = false;

            if (availableButtons == ResponseButtons.StopRetryContinue)
            {
                if (userResponse == ResponseButtons.Stop || userResponse == ResponseButtons.Retry || userResponse == ResponseButtons.Continue)
                {
                    retVal = true;
                }
            }
            else if (availableButtons == ResponseButtons.CancelOK)
            {
                if (userResponse == ResponseButtons.Cancel || userResponse == ResponseButtons.OK)
                {
                    retVal = true;
                }
            }
            else if (availableButtons == ResponseButtons.YesNo)
            {
                if (userResponse == ResponseButtons.Yes || userResponse == ResponseButtons.No)
                {
                    retVal = true;
                }
            }
            else if (availableButtons == userResponse) //grouped buttons handled earlier, so it can be checked directly
            {
                retVal = true;
            }

            return retVal;
        }

        public void TryResolveLog(ErrorResolveModel message)
        {
            if (Logs == null)
            {
                _logger.LogError($"Log list is null");
                return;
            }
            LogMessageModel? log = Logs.FirstOrDefault(model => model.ID.Equals(message.ID));
            if (log == null)
            {
                _logger.LogError($"Could not find received log");
                throw new KeyNotFoundException("Error with the received ID not exists");
            }
            if (!CheckResponseButtonIsValid(log.ValidInteractionButtons, message.UserResponse))
            {
                _logger.LogError($"Invalid response from user. Available responses: {log.ValidInteractionButtons.ToString()} Received: {message.UserResponse}");
                throw new InvalidOperationException($"Invalid response from user. Available responses: {log.ValidInteractionButtons.ToString()} Received: {message.UserResponse}");
            }
            log.UserResponse = message.UserResponse;
            if (log.NeedsInteraction)
            {
                //other responses are not needed
                switch (log.UserResponse)
                {
                    case ResponseButtons.Cancel:
                        log.NextStep = ErrorNextStep.Stop;
                        break;
                    case ResponseButtons.Continue:
                        log.NextStep = ErrorNextStep.Continue;
                        break;
                    case ResponseButtons.Retry:
                        log.NextStep = ErrorNextStep.Retry;
                        break;
                    case ResponseButtons.OK:
                        log.NextStep = ErrorNextStep.Continue;
                        break;
                    case ResponseButtons.Stop:
                        log.NextStep = ErrorNextStep.Stop;
                        break;
                    case ResponseButtons.Yes:
                        if (log.Sender.ToLower().Contains("Init".ToLower()))
                        {
                            if (log.Device == Devices.NIMachine)
                            {
                                log.NextStep = ErrorNextStep.ReInitNI;
                            }
                            else if (log.Device == Devices.Pulser)
                            {
                                log.NextStep = ErrorNextStep.ReInitPulser;
                            }
                            else if (log.Device == Devices.HVPSU)
                            {
                                log.NextStep = ErrorNextStep.ReInitHVPSU;
                            }
                            else if (log.Device == Devices.DAQ)
                            {
                                log.NextStep = ErrorNextStep.ReInitDAQ;
                            }
                            else if (log.Device == Devices.APSU)
                            {
                                log.NextStep = ErrorNextStep.Continue; //it will call enable psu until it is done, then proceed with measurement
                            }
                        }
                        break;
                    case ResponseButtons.No:
                        log.NextStep = ErrorNextStep.Stop;
                        break;
                    default:
                        _logger.LogError($"Unknown or invalid response button received: {log.UserResponse.ToString()}");
                        throw new InvalidDataException($"Unknown or invalid response button received: {log.UserResponse.ToString()}");
                }
            }
            log.Resolved = true;
            log.NeedsInteraction = false;
            switch (log.NextStep)
            {
                case ErrorNextStep.Continue:
                    CheckAndRunNextAsync();
                    break;
                case ErrorNextStep.Retry:
                    SetRetryFailedMeasurement(log.MeasurementType);
                    CheckAndRunNextAsync();
                    break;
                case ErrorNextStep.Stop:
                    StopMeasurement();
                    CurrentTask = TaskTypes.Finished;
                    break;
                case ErrorNextStep.ReInitDAQ:
                    //Add daq init function
                    break;
                case ErrorNextStep.ReInitNI:
                    InitNI();
                    break;
                case ErrorNextStep.ReInitPulser:
                    InitPulser();
                    break;
                case ErrorNextStep.ReInitHVPSU:
                    InitHVPSU();
                    break;
                default:
                    break;
            }
        }

        public CurrentMeasurementDataModel GetSiPMMeasurementData(int blockIndex, int moduleIndex, int arrayIndex, int sipmIndex)
        {
            return serviceState.GetSiPMMeasurementData(blockIndex, moduleIndex, arrayIndex, sipmIndex);
        }

        public string GetSiPMMeasurementStatesJSON()
        {
            if (serviceState == null)
            {
                throw new NullReferenceException("Measurement not started yet");
            }
            return serviceState.GetSiPMMeasurementStatesJSON();
        }

        //Status of individual IV measurements (job is running on NI machine or not
        public MeasurementState IVState
        {
            get
            {
                lock (_lockObject)
                {
                    return globalState.IVModel.MeasurementState;
                }
            }
        }

        //Status of the whole IV measurement
        public MeasurementState GlobalIVState
        {
            get
            {
                lock (_lockObject)
                {
                    return globalState.GlobalIVMeasurementState;
                }
            }
        }

        //Status of individual SPS measurements (job is running on SPS machine or not
        public MeasurementState SPSState
        {
            get
            {
                lock (_lockObject)
                {
                    return globalState.SPSModel.MeasurementState;
                }
            }
        }

        //Status of the whole SPS measurement
        public MeasurementState GLobalSPSState
        {
            get
            {
                lock (_lockObject)
                {
                    return globalState.GlobalSPSMeasurementState;
                }
            }
        }

        public ConnectionState IVConnectionState
        {
            get
            {
                lock (_lockObject)
                {
                    return globalState.IVModel.ConnectionState;
                }
            }
        }

        public ConnectionState SPSConnectionState
        {
            get
            {
                lock (_lockObject)
                {
                    return globalState.SPSModel.ConnectionState;
                }
            }
        }

        public List<TemperaturesArray> Temperatures
        {
            get
            {
                if (Pulser != null)
                {
                    return Pulser.Temperatures.ToList();
                }
                else
                {
                    return new List<TemperaturesArray>();
                }
            }
        }

        public List<CoolerResponse> CoolerStates
        {
            get
            {
                if (Pulser != null)
                {
                    return Pulser.CoolerStates.ToList();
                }
                else
                {
                    return new List<CoolerResponse>();
                }
            }
        }

        public SerialPortState PulserState
        {
            get
            {
                if (Pulser == null)
                {
                    return SerialPortState.Disabled;
                }
                else
                {
                    return Pulser.State;
                }
            }
        }

        //this can be used to start or stop the temperature readings
        public TimeSpan PulserReadingInterval
        {
            get
            {
                if (Pulser != null)
                {
                    return Pulser.UpdatePeriod;
                }
                else
                {
                    return TimeSpan.FromSeconds(0);
                }
            }
            set
            {
                if (Pulser != null)
                {
                    Pulser.ChangeTimerInterval(value);
                }
                else
                {
                    throw new NullReferenceException("Pulser may be disabled or not available");
                }
            }
        }

        public void SetPSU(int block, PSUs psu, bool Enabled, bool askedByUser = false)
        {
            if (Pulser == null)
            {
                _logger.LogDebug($"Can not turn on PSU on block {block} because Pulser is null");
                return;
            }
            else if (!Pulser.Enabled)
            {
                _logger.LogDebug($"Can not turn on PSU on block {block} because Pulser is disabled");
                return;
            }
            if (ivMeasurementSettings.ManualControl && !askedByUser)
            {
                _logger.LogWarning("Manual control enabled but PSU change requested by code");
                return;
            }
            _logger.LogWarning("Manual control disabled. Trying to change PSU and Cooler");
            if (!Enabled && coolerState.GetAPSUState(block))
            {
                bool userOverride = coolerState.GetAPSUStateIsOverridden(block);
                bool module0Enabled = coolerState.GetCoolerSettings(block, 0).Enabled;
                bool module1Enabled = coolerState.GetCoolerSettings(block, 1).Enabled;
                // if both modules disabled and not overridden by user
                if (!userOverride)
                {
                    if (!module0Enabled && !module1Enabled)
                    {
                        Pulser.DisablePSU(block, PSUs.PSU_A);
                        coolerState.SetAPSUState(block, false);
                        CreateAndSendLogMessage("Measurement Service - APSU", $"PSU A for Block {block} disable.", LogMessageType.Info, Devices.APSU, false, ResponseButtons.OK, MeasurementType.Unknown);
                    }
                }
                else if (userOverride && askedByUser)
                {
                    Pulser.DisablePSU(block, PSUs.PSU_A);
                    coolerState.SetAPSUState(block, false);
                    coolerState.SetAPSUStateOverride(block, false);
                    CreateAndSendLogMessage("Measurement Service - APSU", $"PSU A for Block {block} disable.", LogMessageType.Info, Devices.APSU, false, ResponseButtons.OK, MeasurementType.Unknown);
                }
                else
                {
                    _logger.LogDebug($"Can not disable PSU on block {block}. Asked by user: {askedByUser}, User override: {userOverride}, Cooler 0 enabled: {module0Enabled}, Cooler 1 enabled: {module1Enabled}");
                }
                
            }

            if (!coolerState.GetAPSUState(block) && Enabled)
            {
                double voltage;
                double current;
                Pulser.EnablePSU(block, psu, out voltage, out current);
                coolerState.SetAPSUState(block, true);
                coolerState.SetAPSUStateOverride(block, askedByUser);
                CreateAndSendLogMessage("Measurement Service - APSU", $"PSU A for Block {block} enabled. Voltage: {voltage}, Current: {current}", LogMessageType.Info, Devices.APSU, false, ResponseButtons.OK, MeasurementType.Unknown);
            }
        }

        public void SetCooler(CoolerSettingsModel s, bool askedByUser = false)
        {
            if (Pulser == null)
            {
                _logger.LogDebug($"Can not turn on Cooler on block {s.Block} because Pulser is null");
                return;
            }
            else if (!Pulser.Enabled)
            {
                _logger.LogDebug($"Can not turn on Cooler on block {s.Block} because Pulser is disabled");
                return;
            }
            if (ivMeasurementSettings.ManualControl && !askedByUser)
            {
                _logger.LogWarning("Manual control enabled but Cooler change requested by code");
                return;
            }
            if (s.Enabled)
            {
                SetPSU(s.Block, PSUs.PSU_A, true, askedByUser); //it will check if it needs to be enabled
                Pulser.SetCooler(s.Block, s.Module, s.Enabled, s.TargetTemperature, s.FanSpeed);
                coolerState.SetCoolerSettings(s);
                /*
                if (!coolerState.GetCoolerSettings(s.Block, s.Module).Enabled)
                {
                    
                }
                */
            }
            else
            {
                int otherModule = s.Module == 0 ? 1 : 0; //get other module number
                if (coolerState.GetCoolerSettings(s.Block, s.Module).Enabled && coolerState.GetCoolerSettings(s.Block, s.Module).EnabledByUser && askedByUser)
                {
                    Pulser.SetCooler(s.Block, s.Module, s.Enabled, s.TargetTemperature, s.FanSpeed);
                    coolerState.SetCoolerSettings(s);
                }
                else if (coolerState.GetCoolerSettings(s.Block, s.Module).Enabled && !coolerState.GetCoolerSettings(s.Block, s.Module).EnabledByUser)
                {
                    Pulser.SetCooler(s.Block, s.Module, s.Enabled, s.TargetTemperature, s.FanSpeed);
                    coolerState.SetCoolerSettings(s);
                }

                // check if both modules disabled and psu enabled
                if (!coolerState.GetCoolerSettings(s.Block, s.Module).Enabled && !coolerState.GetCoolerSettings(s.Block, otherModule).Enabled)
                {
                    SetPSU(s.Block, PSUs.PSU_A, false, askedByUser); //it will check if it needs to be enabled
                }
            }
        }

        public CoolerSettingsModel GetCooler(int block, int module)
        {
            if (Pulser != null)
            {
                return coolerState.GetCoolerSettings(block, module);
            }
            else
            {
                throw new NullReferenceException("Pulser may be disabled or not available");
            }
        }

        public CoolerStateHandler GetAllCooler()
        {
            if (Pulser != null)
            {
                return coolerState;
            }
            else
            {
                throw new NullReferenceException("Pulser may be disabled or not available");
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    niMachine.Stop();
                }

                disposed = true;
            }
        }

        
    }
}

