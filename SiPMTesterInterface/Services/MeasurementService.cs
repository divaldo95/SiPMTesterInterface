using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SiPMTesterInterface.AnalysisModels;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;
using SiPMTesterInterface.Hubs;
using SiPMTesterInterface.Interfaces;
using SiPMTesterInterface.Libraries;
using SiPMTesterInterface.Models;
using static SiPMTesterInterface.Classes.LEDPulserData;

namespace SiPMTesterInterface.ClientApp.Services
{
    public class MeasurementServiceSettings
    {
        public int BlockCount { get; set; } = 2;
        public int ModuleCount { get; set; } = 2;
        public int ArrayCount { get; set; } = 4;
        public int SiPMCount { get; set; } = 16;
        public bool WaitForTemperatureStabilisation { get; set; } = true;
        public double FridgeTemperature { get; set; } = -30.0;
        public int FridgeFanSpeedPercentage { get; set; } = 50;
        public double BoxTemperature { get; set; } = 25.0;
        public double TemperatureMaxDifference { get; set; } = 1.0;
        public int BoxFanSpeedPercentage { get; set; } = 50;
        public double ExpectedDMMResistance { get; set; } = 10E6;
        public double DMMResistanceMaxDifference { get; set; } = 2E6;

        public MeasurementServiceSettings(IConfiguration config)
        {
            var blockCount = config["MeasurementService:BlockCount"];
            var moduleCount = config["MeasurementService:ModuleCount"];
            var arrayCount = config["MeasurementService:ArrayCount"];
            var sipmCount = config["MeasurementService:SiPMCount"];
            var waitForTemperatureStabilisation = config["MeasurementService:WaitForTemperatureStabilisation"];

            var fridgeTemperature = config["MeasurementService:FridgeTemperature"];
            var fridgeFanSpeedPercentage = config["MeasurementService:FridgeFanSpeedPercentage"];
            var boxTemperature = config["MeasurementService:BoxTemperature"];
            var temperatureMaxDifference = config["MeasurementService:TemperatureMaxDifference"];
            var boxFanSpeedPercentage = config["MeasurementService:BoxFanSpeedPercentage"];

            var expectedDMMResistance = config["MeasurementService:ExpectedDMMResistance"];
            var dmmResistanceMaxDifference = config["MeasurementService:DMMResistanceMaxDifference"];

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
            if (waitForTemperatureStabilisation != null)
            {
                bool val;
                bool.TryParse(waitForTemperatureStabilisation, out val);
                WaitForTemperatureStabilisation = val;
            }

            if (fridgeTemperature != null)
            {
                double val;
                double.TryParse(fridgeTemperature, out val);
                FridgeTemperature = val;
                Console.WriteLine($"Fridge temperature set to {FridgeTemperature}");
            }
            if (fridgeFanSpeedPercentage != null)
            {
                int val;
                int.TryParse(fridgeFanSpeedPercentage, out val);
                FridgeFanSpeedPercentage = val;
                Console.WriteLine($"Fridge Fan set to {FridgeFanSpeedPercentage}");
            }
            if (boxTemperature != null)
            {
                double val;
                double.TryParse(boxTemperature, out val);
                BoxTemperature = val;
                Console.WriteLine($"Box temperature set to {BoxTemperature}");
            }
            if (temperatureMaxDifference != null)
            {
                double val;
                double.TryParse(temperatureMaxDifference, out val);
                TemperatureMaxDifference = val;
                Console.WriteLine($"Box temperature difference set to {TemperatureMaxDifference}");
            }
            if (boxFanSpeedPercentage != null)
            {
                int val;
                int.TryParse(boxFanSpeedPercentage, out val);
                BoxFanSpeedPercentage = val;
                Console.WriteLine($"Box Fan set to {BoxFanSpeedPercentage}");
            }

            if (expectedDMMResistance != null)
            {
                double val;
                double.TryParse(expectedDMMResistance, out val);
                ExpectedDMMResistance = val;
                Console.WriteLine($"Expected DMM resistance set to {ExpectedDMMResistance}");
            }
            if (dmmResistanceMaxDifference != null)
            {
                double val;
                double.TryParse(dmmResistanceMaxDifference, out val);
                DMMResistanceMaxDifference = val;
                Console.WriteLine($"DMM Resistance max difference set to {DMMResistanceMaxDifference}");
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

    public class ExportConfig
    {
        public string BasePath { get; set; } = "~/";

        public ExportConfig(IConfiguration config)
        {
            var bP = config["DefaultMeasurementSettings:ExportConfig:BasePath"];

            if (bP != null)
            {
                BasePath = bP;
            }

            if (BasePath.StartsWith("~/"))
            {
                BasePath = MeasurementService.GetHomePath(BasePath);
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

        long lastMeasurementStartTimestamp = 0;

        public MeasurementServiceSettings MeasurementServiceSettings { get; private set; }
        public IVMeasurementSettings ivMeasurementSettings { get; private set; }
        public DMMMeasurementSettings dmmMeasurementSettings { get; private set; }
        public ExportConfig exportConfig { get; private set; }

        private string currentExportPath = "";

        private bool taskIsRunning = false;
        private TaskTypes taskTypeWaitingForFinish = TaskTypes.Idle;

        private DeviceStatesModel DeviceStates;
        public LEDPulserData PulserValues;
        private ServiceStateHandler serviceState;
        private CoolerStateHandler coolerState;
        private List<CurrentSiPMModel> spsSiPMs;

        private DarkCurrentConfig darkCurrentConfig;
        private ForwardResistanceConfig forwardResistanceConfig;

        private int actualBlock = -1;
        private int actualModule = -1;

        private bool waitForTemperatureStabilisation = false;
        private bool currentlyWaitingForTemperatureStabilisation = false;

        private readonly IConfiguration Configuration;

        private readonly ResistanceCompensationValues resistanceValues;

        private readonly IHubContext<UpdatesHub, IStateContext> _hubContext;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MeasurementService> _logger;

        public long StartTimestamp { get; private set; } = 0;
        public long EndTimestamp { get; private set; } = 0;

        private BTLDBHandler databaseHandler = new BTLDBHandler();

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

        public MeasurementStartModel CurrentRun
        {
            get
            {
                return globalState.CurrentRun;
            }
        }

        private DateTime utcDate;

        string outputBaseDir;

        private void PopulateServiceState()
        {
            if (!DeviceStates.IsAllEnabledDeviceStarted() && false)
            {
                CreateAndSendLogMessage("PopulateServiceState", "MeasurementService has devices which are not started", LogMessageType.Error, Devices.Unknown, false, ResponseButtons.OK, MeasurementType.Unknown);
                throw new ApplicationException("MeasurementService has devices which are not started");
            }
            serviceState = new ServiceStateHandler(MeasurementServiceSettings.BlockCount, MeasurementServiceSettings.ModuleCount,
                                                        MeasurementServiceSettings.ArrayCount, MeasurementServiceSettings.SiPMCount);
            serviceState.OnActiveSiPMsChanged += ServiceState_OnActiveSiPMsChanged;

            utcDate = DateTime.UtcNow;
            outputBaseDir = Path.Combine(FilePathHelper.GetCurrentDirectory(), "Measurements", utcDate.ToString("yyyyMMddHHmmss"));
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
                if (c.SiPMMeasurementDetails.IV != 0)
                {
                    c.Checks.SelectedForMeasurement = true;
                }
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
            if (logType == LogMessageType.Debug)
            {
                _logger.LogDebug($"{sender} - {message}");
            }
            else if (logType == LogMessageType.Error)
            {
                _logger.LogError($"{sender} - {message}");
            }
            else if (logType == LogMessageType.Info)
            {
                _logger.LogInformation($"{sender} - {message}");
            }
            else if (logType == LogMessageType.Warning)
            {
                _logger.LogWarning($"{sender} - {message}");
            }
            else if (logType == LogMessageType.Fatal)
            {
                _logger.LogCritical($"{sender} - {message}");
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

        public double GetCompensatedOperatingVoltage(MeasurementOrderModel next)
        {
            double Vop = 0.0;

            List<double> temps;

            Models.Array arr = globalState.CurrentRun.Blocks[next.SiPM.Block]
                                .Modules[next.SiPM.Module]
                                .Arrays[next.SiPM.Array];

            SiPM sipm = arr.SiPMs[next.SiPM.SiPM];

            if (next.Type == MeasurementType.IVMeasurement || next.Type == MeasurementType.ForwardResistanceMeasurement || next.Type == MeasurementType.DarkCurrentMeasurement)
            {
                temps = TemperatureSelectHelper.GetTemperatures(Temperatures, next.SiPM.Block, next.SiPM.Module, next.SiPM.Array, next.SiPM.SiPM, lastMeasurementStartTimestamp);
            }
            else
            {
                throw new ArgumentException($"Unknown measurement type {next.Type} to get compensated operating voltage");
            }

            double toTemperature;
            if (temps.Count > 0)
            {
                toTemperature = temps.Average();
                _logger.LogInformation($"Using average of measured temperatures for compensation: {toTemperature}C");
            }
            else if (coolerState.GetCoolerSettings(next.SiPM.Block, next.SiPM.Module).Enabled)
            {
                toTemperature = coolerState.GetCoolerSettings(next.SiPM.Block, next.SiPM.Module).TargetTemperature;
                _logger.LogInformation($"Using cooler target temperature for compensation: {toTemperature}C");
            }
            else
            {
                toTemperature = 25;
                _logger.LogInformation($"Using preset temperature for compensation: {toTemperature}C");
            }

            _logger.LogInformation($"SiPM ({next.SiPM.Block},{next.SiPM.Module},{next.SiPM.Array},{next.SiPM.SiPM}) temperature to compensate Vop to is {toTemperature}");

            if (toTemperature < 20 || toTemperature > 30)
            {
                throw new InvalidDataException($"SiPM ({next.SiPM.Block},{next.SiPM.Module},{next.SiPM.Array},{next.SiPM.SiPM}) temperature to compensate Vop to is {toTemperature} which is out of bounds (20 < T < 30)");
            }

            if (sipm.OperatingVoltage > 0)
            {
                Vop = SiPMDatasheetHandler.GetCompensatedOperatingVoltage(sipm.OperatingVoltage, toTemperature);
            }
            else
            {
                Vop = SiPMDatasheetHandler.GetCompensatedOperatingVoltage(SiPMDatasheetHandler.GetSiPMVop(arr.Barcode, next.SiPM.SiPM), toTemperature); //backend added later
            }

            //double VopDiff = Math.Abs(Vop - sipm.OperatingVoltage);
            //if (VopDiff > 5.0) //difference larger than 5V (too much?)
            if (!Vop.IsBetweenLimits(sipm.OperatingVoltage, 5.0))
            {
                throw new InvalidDataException($"SiPM ({next.SiPM.Block},{next.SiPM.Module},{next.SiPM.Array},{next.SiPM.SiPM}) compensated Vop differs from the given one which is out of bounds (VopDiff < 5.0)");
            }

            return Math.Round(Vop, 2, MidpointRounding.AwayFromZero);
        }

        private void HandleBlockChanges(int currentlyUsedBlock, int nextUsedBlock)
        {
            CoolerSettingsModel s = new CoolerSettingsModel();
            // disable if next is -1 -1 as the measurement is ending

            //turn off previous modules and block
            if (currentlyUsedBlock >= 0)
            {
                DisableBlockWithModules(currentlyUsedBlock);
            }

            EnableBlockWithModules(nextUsedBlock);
        }

        private void EnableBlockWithModules(int block)
        {
            var FirstModuleCooler = coolerState.GetCopyOfCoolerSettings(block, 0);
            if (!FirstModuleCooler.Enabled)
            {
                // enable next
                FirstModuleCooler.Enabled = true;
                SetCooler(FirstModuleCooler);
            }

            var SecondModuleCooler = coolerState.GetCopyOfCoolerSettings(block, 1);
            if (!SecondModuleCooler.Enabled)
            {
                // enable next
                SecondModuleCooler.Enabled = true;
                SetCooler(SecondModuleCooler);
            }
        }

        private void DisableBlockWithModules(int block)
        {
            var FirstPrevModuleCooler = coolerState.GetCopyOfCoolerSettings(block, 0);
            var SecondPrevModuleCooler = coolerState.GetCopyOfCoolerSettings(block, 1);

            FirstPrevModuleCooler.Enabled = false;
            SetCooler(FirstPrevModuleCooler);

            SecondPrevModuleCooler.Enabled = false;
            SetCooler(SecondPrevModuleCooler);
        }

        public void CheckAndRunNext()
        {
            MeasurementOrderModel nextMeasurement;

            //Store the last timestamp a measurement starts to get the right temperature values
            //Make sure to have at least one measurement
            lastMeasurementStartTimestamp = TimestampHelper.GetUTCTimestamp() - (int)Pulser.UpdatePeriod.TotalSeconds;

            //turn off pulser, disconnect all relays
            EndTimestamp = TimestampHelper.GetUTCTimestamp();//just to make sure something is saved

            if (MeasurementStopped)
            {
                CurrentTask = TaskTypes.Idle;
                TryEndMeasurement();
                serviceState.ActiveSiPMs = new List<CurrentSiPMModel>(); //empty list 
                _logger.LogWarning($"Measurement stopped");
                return;
            }

            var logList = GetAttentionNeededLogs();

            if (logList.Count > 0)
            {
                _logger.LogWarning($"{logList.Count} logs waiting for user response");
                return;
            }

            if (taskIsRunning)
            {
                _logger.LogWarning("Another task is running, but CheckAndRunNext() is called");
                return;
            }
            
            Console.WriteLine("Checking new iteration...");
            if (GetNextTask(out nextMeasurement))
            {
                serviceState.ActiveSiPMs = new List<CurrentSiPMModel>()
                {
                    nextMeasurement.SiPM
                };
                Console.WriteLine($"Next task is {nextMeasurement.Task}");
                Console.WriteLine($"Next measurement type is {nextMeasurement.Type}");
                _logger.LogDebug($"(Block={nextMeasurement.SiPM.Block}, Module={nextMeasurement.SiPM.Module}, TempStable={coolerState.GetCoolerSettings(nextMeasurement.SiPM.Block, nextMeasurement.SiPM.Module).State.IsTemperatureStable})");

                //It will be marked as done when received the data
                actualBlock = nextMeasurement.SiPM.Block;
                actualModule = nextMeasurement.SiPM.Module;

                if (nextMeasurement.Type != MeasurementType.Unknown && nextMeasurement.Type != MeasurementType.DMMResistanceMeasurement &&
                    (nextMeasurement.Task == TaskTypes.DarkCurrent || nextMeasurement.Task == TaskTypes.ForwardResistance || nextMeasurement.Task == TaskTypes.IV))
                {
                    if (MeasurementServiceSettings.WaitForTemperatureStabilisation)
                    {
                        waitForTemperatureStabilisation = true;
                    }
                }
                else
                {
                    waitForTemperatureStabilisation = false;
                }

                //wait for temperature stabilisation
                if (Pulser != null && Pulser.Enabled)
                {
                    bool isStable = coolerState.GetCoolerSettings(actualBlock, actualModule).State.IsTemperatureStable;
                    _logger.LogDebug($"WaitForTemperatureStabilisation={waitForTemperatureStabilisation}, Block={actualBlock}, Module={actualModule}, TempStable={isStable}");
                    if (waitForTemperatureStabilisation && !isStable)
                    {
                        currentlyWaitingForTemperatureStabilisation = true;
                        _logger.LogDebug($"Waiting for temperature stabilisation... (Block={actualBlock}, Module={actualModule}, TempStable={isStable})");
                        CurrentTask = TaskTypes.TemperatureStabilisation;
                        return;
                    }
                    else
                    {
                        _logger.LogDebug($"Temperature stabilised (Block={actualBlock}, Module={actualModule}, TempStable={isStable}, WaitForStabilisation={waitForTemperatureStabilisation})");
                        currentlyWaitingForTemperatureStabilisation = false;
                    }
                }

                if (nextMeasurement.Task == TaskTypes.BlockDisable)
                {
                    try
                    {
                        _logger.LogDebug($"Disabling block {nextMeasurement.SiPM.Block}...");
                        taskIsRunning = true;
                        taskTypeWaitingForFinish = nextMeasurement.Task;
                        DisableBlockWithModules(nextMeasurement.SiPM.Block);
                        MarkCurrentTaskDone();
                        taskIsRunning = false;
                        taskTypeWaitingForFinish = TaskTypes.Idle;
                        _logger.LogDebug($"Block {nextMeasurement.SiPM.Block} disabled");
                    }
                    catch (Exception ex)
                    {
                        CreateAndSendLogMessage("Measurement Service - Disable Block", ex.Message + " - Try again?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                    }
                    //check and run next, it will go to next step if it is done properly
                    CheckAndRunNextAsync();
                    return;
                    
                }
                else if (nextMeasurement.Task == TaskTypes.BlockEnable)
                {
                    try
                    {
                        _logger.LogDebug($"Enabling block {nextMeasurement.SiPM.Block}...");
                        taskIsRunning = true;
                        taskTypeWaitingForFinish = nextMeasurement.Task;
                        EnableBlockWithModules(nextMeasurement.SiPM.Block);
                        MarkCurrentTaskDone();
                        taskIsRunning = false;
                        taskTypeWaitingForFinish = TaskTypes.Idle;
                        _logger.LogDebug($"Block {nextMeasurement.SiPM.Block} enabled");
                    }
                    catch (Exception ex)
                    {
                        CreateAndSendLogMessage("Measurement Service - Enable Block", ex.Message + " - Try again?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                    }
                    //check and run next, it will go to next step if it is done properly
                    CheckAndRunNextAsync();
                    return;
                }

                else if (nextMeasurement.Type == MeasurementType.DMMResistanceMeasurement)
                {
                    CurrentTask = TaskTypes.DMMResistance;
                    NIDMMStartModel? niDMMStart = nextMeasurement.StartModel as NIDMMStartModel;
                    if (niDMMStart == null)
                    {
                        throw new NullReferenceException("NIDMMStartModel can not be null");
                    }
                    niDMMStart.DMMResistance = globalState.CurrentRun.DMMResistance;
                    try
                    {
                        Pulser.SetMode(nextMeasurement.SiPM.Block, nextMeasurement.SiPM.Module, nextMeasurement.SiPM.Array, nextMeasurement.SiPM.SiPM, MeasurementMode.MeasurementModes.DMMResistanceMeasurement, new[] { 0, 0, 0, 0 });
                    }
                    catch (SerialTimeoutLimitReachedException ex)
                    {
                        CreateAndSendLogMessage("Measurement Service - DMM Resistance - Pulser", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                        return;
                    }
                    catch (Exception ex)
                    {
                        globalState.GlobalIVMeasurementState = MeasurementState.Error;
                        CreateAndSendLogMessage("CheckAndRunNext - DMM Resistance - Pulser", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.DMMResistanceMeasurement);
                        return;
                    }
                    niDMMStart.DMMResistance.Iterations = dmmMeasurementSettings.Iterations;
                    niDMMStart.DMMResistance.Voltage = dmmMeasurementSettings.Voltage;
                    niDMMStart.DMMResistance.CorrectionPercentage = dmmMeasurementSettings.CorrectionPercentage;
                    taskTypeWaitingForFinish = TaskTypes.DMMResistance;
                    niMachine?.StartDMMResistanceMeasurement(niDMMStart);
                }

                else if (nextMeasurement.Type == MeasurementType.DarkCurrentMeasurement)
                {
                    CurrentTask = TaskTypes.DarkCurrent;
                    NIVoltageAndCurrentStartModel? niVIStart = nextMeasurement.StartModel as NIVoltageAndCurrentStartModel;
                    CurrentMeasurementDataModel c = serviceState.GetSiPMMeasurementData(nextMeasurement.SiPM.Block, nextMeasurement.SiPM.Module, nextMeasurement.SiPM.Array, nextMeasurement.SiPM.SiPM);
                    if (niVIStart == null)
                    {
                        throw new NullReferenceException("NIVoltageAndCurrentStartModel can not be null on DC");
                    }
                    if (niVIStart.MeasurementType == VoltageAndCurrentMeasurementTypes.LeakageCurrent)
                    {
                        double Vop;
                        try
                        {
                            Vop = GetCompensatedOperatingVoltage(nextMeasurement);
                        }
                        catch (InvalidDataException ex)
                        {
                            CreateAndSendLogMessage("Temperature Compensator",
                                $"{ex.Message}. Waiting for new temperature measurements...",
                                LogMessageType.Error, Devices.Pulser, false, ResponseButtons.OK, MeasurementType.Unknown);
                            Thread.Sleep(PulserReadingInterval);
                            CheckAndRunNextAsync();
                            return;
                        }

                        niVIStart.FirstIteration.CurrentLimit = darkCurrentConfig.LeakageCurrent.FirstCurrentLimit;
                        niVIStart.FirstIteration.CurrentLimitRange = darkCurrentConfig.LeakageCurrent.FirstCurrentLimitRange;
                        niVIStart.FirstIteration.Iterations = darkCurrentConfig.Iterations;
                        niVIStart.FirstIteration.Voltage = Vop + darkCurrentConfig.LeakageCurrent.FirstVoltageOffset;
                        niVIStart.FirstIteration.VoltageRange = darkCurrentConfig.LeakageCurrent.FirstVoltageRange;

                        niVIStart.SecondIteration.CurrentLimit = darkCurrentConfig.LeakageCurrent.SecondCurrentLimit;
                        niVIStart.SecondIteration.CurrentLimitRange = darkCurrentConfig.LeakageCurrent.SecondCurrentLimitRange;
                        niVIStart.SecondIteration.Iterations = darkCurrentConfig.Iterations;
                        niVIStart.SecondIteration.Voltage = Vop + darkCurrentConfig.LeakageCurrent.SecondVoltageOffset;
                        niVIStart.SecondIteration.VoltageRange = darkCurrentConfig.LeakageCurrent.SecondVoltageRange;

                        //Save ID
                        c.DarkCurrentResult.LeakageCurrentResult.Identifier = niVIStart.Identifier;
                    }
                    else if (niVIStart.MeasurementType == VoltageAndCurrentMeasurementTypes.DarkCurrent)
                    {
                        double Vop;
                        try
                        {
                            Vop = GetCompensatedOperatingVoltage(nextMeasurement);
                        }
                        catch (InvalidDataException ex)
                        {
                            CreateAndSendLogMessage("Temperature Compensator",
                                $"{ex.Message}. Waiting for new temperature measurements...",
                                LogMessageType.Error, Devices.Pulser, false, ResponseButtons.OK, MeasurementType.Unknown);
                            Thread.Sleep(PulserReadingInterval);
                            CheckAndRunNextAsync();
                            return;
                        }

                        niVIStart.FirstIteration.CurrentLimit = darkCurrentConfig.DarkCurrent.FirstCurrentLimit;
                        niVIStart.FirstIteration.CurrentLimitRange = darkCurrentConfig.DarkCurrent.FirstCurrentLimitRange;
                        niVIStart.FirstIteration.Iterations = darkCurrentConfig.Iterations;
                        niVIStart.FirstIteration.Voltage = Vop + darkCurrentConfig.DarkCurrent.FirstVoltageOffset;
                        niVIStart.FirstIteration.VoltageRange = darkCurrentConfig.DarkCurrent.FirstVoltageRange;

                        niVIStart.SecondIteration.CurrentLimit = darkCurrentConfig.DarkCurrent.SecondCurrentLimit;
                        niVIStart.SecondIteration.CurrentLimitRange = darkCurrentConfig.DarkCurrent.SecondCurrentLimitRange;
                        niVIStart.SecondIteration.Iterations = darkCurrentConfig.Iterations;
                        niVIStart.SecondIteration.Voltage = Vop + darkCurrentConfig.DarkCurrent.SecondVoltageOffset;
                        niVIStart.SecondIteration.VoltageRange = darkCurrentConfig.DarkCurrent.SecondVoltageRange;

                        //Save ID
                        c.DarkCurrentResult.DarkCurrentResult.Identifier = niVIStart.Identifier;
                    }

                    try
                    {
                        //Change modes accordingly
                        if (niVIStart.MeasurementType == VoltageAndCurrentMeasurementTypes.LeakageCurrent)
                            Pulser.SetMode(nextMeasurement.SiPM.Block, nextMeasurement.SiPM.Module, nextMeasurement.SiPM.Array, nextMeasurement.SiPM.SiPM, MeasurementMode.MeasurementModes.LeakageCurrent, new[] { 0, 0, 0, 0 });
                        else if (niVIStart.MeasurementType == VoltageAndCurrentMeasurementTypes.DarkCurrent)
                            Pulser.SetMode(nextMeasurement.SiPM.Block, nextMeasurement.SiPM.Module, nextMeasurement.SiPM.Array, nextMeasurement.SiPM.SiPM, MeasurementMode.MeasurementModes.DarkCurrent, new[] { 0, 0, 0, 0 });
                    }
                    catch (SerialTimeoutLimitReachedException ex)
                    {
                        CreateAndSendLogMessage("Measurement Service - VI Measurement - Pulser", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                        return;
                    }
                    catch (Exception ex)
                    {
                        globalState.GlobalIVMeasurementState = MeasurementState.Error;
                        CreateAndSendLogMessage("CheckAndRunNext - VI Measurement", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.DarkCurrentMeasurement);
                        return;
                    }
                    taskTypeWaitingForFinish = TaskTypes.DarkCurrent;
                    niMachine?.StartVIMeasurement(niVIStart);
                    taskIsRunning = true;
                    return;
                }

                else if (nextMeasurement.Type == MeasurementType.ForwardResistanceMeasurement)
                {
                    CurrentTask = TaskTypes.ForwardResistance;
                    NIVoltageAndCurrentStartModel? niVIStart = nextMeasurement.StartModel as NIVoltageAndCurrentStartModel;
                    CurrentMeasurementDataModel c = serviceState.GetSiPMMeasurementData(nextMeasurement.SiPM.Block, nextMeasurement.SiPM.Module, nextMeasurement.SiPM.Array, nextMeasurement.SiPM.SiPM);
                    if (niVIStart == null)
                    {
                        throw new NullReferenceException("NIVoltageAndCurrentStartModel can not be null on FR");
                    }

                    niVIStart.FirstIteration.CurrentLimit = forwardResistanceConfig.FirstCurrentLimit;
                    niVIStart.FirstIteration.CurrentLimitRange = forwardResistanceConfig.FirstCurrentLimitRange;
                    niVIStart.FirstIteration.Iterations = forwardResistanceConfig.Iterations;
                    niVIStart.FirstIteration.Voltage = forwardResistanceConfig.FirstVoltage;
                    niVIStart.FirstIteration.VoltageRange = forwardResistanceConfig.FirstVoltageRange;

                    niVIStart.SecondIteration.CurrentLimit = forwardResistanceConfig.SecondCurrentLimit;
                    niVIStart.SecondIteration.CurrentLimitRange = forwardResistanceConfig.SecondCurrentLimitRange;
                    niVIStart.SecondIteration.Iterations = forwardResistanceConfig.Iterations;
                    niVIStart.SecondIteration.Voltage = forwardResistanceConfig.SecondVoltage;
                    niVIStart.SecondIteration.VoltageRange = forwardResistanceConfig.SecondVoltageRange;

                    //Save ID
                    c.ForwardResistanceResult.Result.Identifier = niVIStart.Identifier;

                    try
                    {
                        Pulser.SetMode(nextMeasurement.SiPM.Block, nextMeasurement.SiPM.Module, nextMeasurement.SiPM.Array, nextMeasurement.SiPM.SiPM, MeasurementMode.MeasurementModes.ForwardResistance, new[] { 0, 0, 0, 0 });
                    }
                    catch (SerialTimeoutLimitReachedException ex)
                    {
                        CreateAndSendLogMessage("Measurement Service - Forward Resistance - Pulser", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.ForwardResistanceMeasurement);
                        return;
                    }
                    catch (Exception ex)
                    {
                        globalState.GlobalIVMeasurementState = MeasurementState.Error;
                        CreateAndSendLogMessage("CheckAndRunNext - Forward Resistance", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.YesNo, MeasurementType.DMMResistanceMeasurement);
                        return;
                    }
                    taskTypeWaitingForFinish = TaskTypes.ForwardResistance;
                    niMachine?.StartVIMeasurement(niVIStart);
                    taskIsRunning = true;
                }

                else if (nextMeasurement.Type == MeasurementType.IVMeasurement)
                {
                    CurrentTask = TaskTypes.IV;

                    NIIVStartModel niIVStart = nextMeasurement.StartModel as NIIVStartModel;

                    if (niIVStart.Voltages.Count == 0)
                    {
                        double Vop;
                        try
                        {
                            Vop = GetCompensatedOperatingVoltage(nextMeasurement);
                        }
                        catch (InvalidDataException ex)
                        {
                            CreateAndSendLogMessage("Temperature Compensator",
                                $"{ex.Message}. Waiting for new temperature measurements...",
                                LogMessageType.Error, Devices.Pulser, false, ResponseButtons.OK, MeasurementType.Unknown);
                            Thread.Sleep(PulserReadingInterval);
                            CheckAndRunNextAsync();
                            return;
                        }
                        niIVStart.Voltages = SiPMDatasheetHandler.GenerateIVVoltageList(Vop); //compensates to target temperature and generates IV list
                        _logger.LogInformation($"IV voltages generated from operating voltage ({niIVStart.OperatingVoltage}): {string.Join(',', niIVStart.Voltages)}");
                    }
                    int[] pulserLEDValues = new[]
                    {
                        //LEDValueHelper.GetPulserValueForSiPM(ivSiPMs[0], PulserValues).PulserValue,
                        PulserValues.GetPulserValue(nextMeasurement.SiPM),
                        0,
                        0,
                        0,
                    };
                    try
                    {
                        Pulser?.SetMode(nextMeasurement.SiPM.Block, nextMeasurement.SiPM.Module, nextMeasurement.SiPM.Array, nextMeasurement.SiPM.SiPM, MeasurementMode.MeasurementModes.IV,
                            new[] { pulserLEDValues[0], pulserLEDValues[1], pulserLEDValues[2], pulserLEDValues[3] });
                    }
                    catch (Exception ex)
                    {
                        globalState.GlobalIVMeasurementState = MeasurementState.Error;
                        CreateAndSendLogMessage("CheckAndRunNext - IV", ex.Message, LogMessageType.Error, Devices.Pulser, true, ResponseButtons.StopRetryContinue, MeasurementType.IVMeasurement);
                        return;
                    }
                    CurrentMeasurementDataModel c = serviceState.GetSiPMMeasurementData(nextMeasurement.SiPM.Block, nextMeasurement.SiPM.Module, nextMeasurement.SiPM.Array, nextMeasurement.SiPM.SiPM);
                    c.IVMeasurementID = niIVStart.Identifier;
                    taskTypeWaitingForFinish = TaskTypes.IV;
                    niMachine?.StartIVMeasurement(niIVStart);
                    taskIsRunning = true;
                }

                else if (nextMeasurement.Type == MeasurementType.TemperatureMeasurement)
                {
                    CurrentTask = TaskTypes.TemperatureMeasurement;
                    taskTypeWaitingForFinish = TaskTypes.TemperatureMeasurement;
                    taskIsRunning = true;
                    MeasureAndAddTemperature(nextMeasurement.SiPM);
                }
                else if (nextMeasurement.Type == MeasurementType.Analysis)
                {
                    CurrentTask = TaskTypes.Analysis;
                    taskTypeWaitingForFinish = TaskTypes.Analysis;
                    taskIsRunning = true;
                    AnalyseSiPM(nextMeasurement.SiPM);
                }
                //WIP
                else if (nextMeasurement.Type == MeasurementType.SPSMeasurement)
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
                    //spsSiPMs = sipms;
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
                            //case 3:
                            //    mode = MeasurementMode.MeasurementModes.QuadSPS;
                            //    break;
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

        public SiPMArrayDto GetArrayData(string sn)
        {
            return databaseHandler.GetArrayDataBySN(sn);
        }

        public void TryEndMeasurement()
        {
            try
            {
                if (Pulser != null && Pulser.Enabled)
                {
                    if (coolerState.GetAPSUState(0))
                    {
                        Pulser.SetMode(0, 0, 0, 0, MeasurementMode.MeasurementModes.Off, new[] { 0, 0, 0, 0 });
                    }

                    List<int> activeBlocks = new List<int>();
                    activeBlocks.AddRange(Pulser.ActiveBlocks);

                    foreach (var activeBlock in activeBlocks)
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
                taskIsRunning = false;
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

        private Task MeasureAndAddTemperature(CurrentSiPMModel sipm)
        {
            Task t = Task.Run(() =>
            {
                try
                {
                    int retryNum = 10;
                    TemperaturesArray? temperatures = null;
                    // try to read temperature 10 times max
                    CurrentMeasurementDataModel data = serviceState.GetSiPMMeasurementData(sipm.Block, sipm.Module, sipm.Array, sipm.SiPM);
                    _logger.LogDebug($"SiPM ({data.SiPMLocation.Block}, {data.SiPMLocation.Module}, {data.SiPMLocation.Array}, {data.SiPMLocation.SiPM}) has {data.Temperatures.Count} temperatures");
                    for (int i = 0; i < retryNum; i++)
                    {
                        if (Pulser == null)
                        {
                            break;
                        }
                        temperatures = Pulser.ReadTemperatures(data.SiPMLocation.Block);
                        _logger.LogDebug(temperatures.ToString());
                        if (temperatures.Validate(data.SiPMLocation.Module))
                        {
                            break;
                        }
                        else
                        {
                            _logger.LogInformation($"Temperature validation error ({i}/{retryNum})");
                            Thread.Sleep(400); // Wait a bit
                        }
                    }

                    if (temperatures != null && temperatures.Validate(data.SiPMLocation.Module))
                    {
                        data.Temperatures.Add(temperatures);
                        _logger.LogInformation($"SiPM ({data.SiPMLocation.Block}, {data.SiPMLocation.Module}, {data.SiPMLocation.Array}, {data.SiPMLocation.SiPM}) temperatures are validated");
                    }
                    else if (temperatures != null)
                    {
                        CreateAndSendLogMessage("Temperature Reader",
                            $"SiPM ({data.SiPMLocation.Block}, {data.SiPMLocation.Module}, {data.SiPMLocation.Array}, {data.SiPMLocation.SiPM}) temperatures list has errors after {retryNum} retries. - Do you want to try it again?",
                            LogMessageType.Warning,
                            Devices.Pulser,
                            true,
                            ResponseButtons.YesNo,
                            MeasurementType.TemperatureMeasurement);
                        return;
                    }
                    else if (Pulser != null && temperatures == null)
                    {
                        CreateAndSendLogMessage("Temperature Reader",
                            $"SiPM ({data.SiPMLocation.Block}, {data.SiPMLocation.Module}, {data.SiPMLocation.Array}, {data.SiPMLocation.SiPM}) temperatures list can not be read by {retryNum} retries. - Do you want to try it again?",
                            LogMessageType.Warning,
                            Devices.Pulser,
                            true,
                            ResponseButtons.YesNo,
                            MeasurementType.TemperatureMeasurement);
                        return;
                    }

                    
                }
                catch (Exception ex)
                {
                    CreateAndSendLogMessage("Temperature Reader",
                            $"SiPM ({sipm.Block}, {sipm.Module}, {sipm.Array}, {sipm.SiPM}): {ex.Message} - Do you want to try it again?",
                            LogMessageType.Error,
                            Devices.Pulser,
                            true,
                            ResponseButtons.YesNo,
                            MeasurementType.TemperatureMeasurement);
                    _logger.LogDebug(ex.StackTrace);
                    return;
                }

                if (taskTypeWaitingForFinish == TaskTypes.TemperatureMeasurement)
                {
                    taskIsRunning = false;
                    MarkCurrentTaskDone();
                    CheckAndRunNextAsync();
                    _logger.LogDebug("Temperature measurement done");
                }
                _logger.LogDebug("Temperature measurement ended");
            });
            return t;
        }

        private Task RunAnalysis(CurrentMeasurementDataModel data)
        {
            Task t = Task.Run(() =>
            {
                
                try
                {
                    _logger.LogInformation("Starting analysis task...");
                    bool voltageCheck = data.IVResult.DMMVoltage.Count > 0;
                    data.Checks.IVVoltageCheckOK = voltageCheck;
                    bool currentCheck = false;

                    for (int i = 0; i < data.IVResult.DMMVoltage.Count; i++)
                    {
                        if (data.IVResult.DMMVoltage[i] < 25.0)
                        {
                            voltageCheck = false;
                        }
                    }

                    for (int i = 0; i < data.IVResult.SMUCurrent.Count; i++)
                    {
                        if (data.IVResult.SMUCurrent[i] > 10E-6)
                        {
                            currentCheck = true;
                        }
                    }

                    data.IVResult.AnalysationResult.IsCurrentCheckOK = voltageCheck && currentCheck;
                    data.Checks.IVCurrentCheckOK = currentCheck;
                    data.IVResult.AnalysationResult.Analysed = true;

                    if (data.IVResult.AnalysationResult.IsCurrentCheckOK)
                    {
                        string outputPath = Path.Combine(outputBaseDir, data.Barcode, "IVAnalysisResult");
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
                    
                    FileOperationHelper.SaveIVResult(data, outputBaseDir);
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
                _hubContext.Clients.All.ReceiveSiPMChecksChange(data.SiPMLocation, data.Checks);
                _logger.LogInformation("Analysis task done");

            });
            return t;
        }

        public void StartMeasurement(MeasurementStartModel measurementData)
        {
            PrepareMeasurement(measurementData);
            PopulateServiceState(); //store measurements
            if (FinalMeasurementOrder.Count > 0)
            {
                CreateAndSendLogMessage("StartMeasurement", "Measurement is starting...", LogMessageType.Info, Devices.Unknown, false, ResponseButtons.OK, MeasurementType.Unknown);
            }
            else
            {
                CreateAndSendLogMessage("StartMeasurement", "Empty measurement list. Maybe none of the SiPMs are selected or has empty or invalid voltage list/Vop.", LogMessageType.Info, Devices.Unknown, false, ResponseButtons.OK, MeasurementType.Unknown);
                CurrentTask = TaskTypes.Idle;
            }
            taskIsRunning = false;
            taskTypeWaitingForFinish = TaskTypes.Idle;
            MeasurementStopped = false;
            CheckAndRunNextAsync();
            StartTimestamp = TimestampHelper.GetUTCTimestamp();
        }

        public void StopMeasurement()
        {

            MeasurementStopped = true;
            CheckAndRunNextAsync();
            niMachine?.StopMeasurement();
            //Pulser?.DisablePSU(0, PSUs.PSU_A);
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
                niMachine.OnVIMeasurementDataReceived += NiMachine_OnVIMeasurementDataReceived;
                niMachine.OnOngoingMeasurementDataReceived += NiMachine_OnOngoingMeasurementDataReceived;
                niMachine.OnMeasurementStartFail += NiMachine_OnMeasurementStartFail;

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

        private void NiMachine_OnMeasurementStartFail(object? sender, MeasurementStartEventArgs e)
        {
            CreateAndSendLogMessage("Measurement Service - Measurement start failed", e.Response.ErrorMessage + " - Do you want to retry?", LogMessageType.Error, Devices.NIMachine, true, ResponseButtons.YesNo, MeasurementType.Unknown);
        }

        private void NiMachine_OnOngoingMeasurementDataReceived(object? sender, OngoingMeasurementEventArgs e)
        {
            _logger.LogInformation("Ongoing measurement in progress: " + JsonConvert.SerializeObject(e));
        }

        private void NiMachine_OnVIMeasurementDataReceived(object? sender, VoltageAndCurrenteasurementDataReceivedEventArgs e)
        {
            _logger.LogInformation("VIMeasurementReceived");
            CurrentMeasurementDataModel c;

            if (e.Data.StartModel.MeasurementType == VoltageAndCurrentMeasurementTypes.DarkCurrent)
            {
                if (serviceState.GetSiPMDataByDarkCurrentID(e.Data.Identifier, out c))
                {
                    c.DarkCurrentResult.DarkCurrentResult = e.Data;
                    c.DarkCurrentResult.DarkCurrentResult.Temperatures = Temperatures.Where(item => item.Timestamp >= c.DarkCurrentResult.DarkCurrentResult.StartTimestamp - Pulser.UpdatePeriod.TotalSeconds && item.Timestamp <= c.DarkCurrentResult.DarkCurrentResult.EndTimestamp).ToList();
                    FileOperationHelper.SaveIVResult(c, outputBaseDir); //it will overwrite everything
                    if (taskTypeWaitingForFinish == TaskTypes.DarkCurrent)
                    {
                        if (e.Data.ErrorHappened)
                        {
                            c.Checks.IDarkDone = true;
                            c.Checks.IDarkOK = false;
                            _logger.LogError($"{e.Data.ErrorMessage}");
                            CreateAndSendLogMessage("Failed VI measurement", $"VI measurement (DC) for {c.SiPMLocation.Block}, {c.SiPMLocation.Module}, {c.SiPMLocation.Array}, {c.SiPMLocation.SiPM} is failed. Reason: {c.IVResult.ErrorMessage}. Choose the next step!", LogMessageType.Error, Devices.NIMachine, true, ResponseButtons.StopRetryContinue, MeasurementType.DarkCurrentMeasurement);
                            return;
                        }
                        else
                        {
                            c.Checks.IDarkDone = true;
                            // Check these limits
                            try
                            {
                                //if (c.DarkCurrentResult.FirstDarkCurrentCompensated < 5E-7 && c.DarkCurrentResult.FirstDarkCurrentCompensated > -1E-8)
                                if (!c.DarkCurrentResult.FirstDarkCurrentCompensated.IsOutOfBoundaries(-1E-8, 5E-7))
                                {
                                    c.Checks.IDarkOK = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogInformation("Dark Current has a null value. Probably one of its measurements is not available yet.");
                            }
                            taskIsRunning = false;
                            MarkCurrentTaskDone();
                        }
                        _hubContext.Clients.All.ReceiveSiPMChecksChange(c.SiPMLocation, c.Checks);
                    }
                }
                else
                {
                    _logger.LogError($"Unknown measurement identifier on DarkCurrentMeasurement DC");
                }
                
            }
            else if (e.Data.StartModel.MeasurementType == VoltageAndCurrentMeasurementTypes.LeakageCurrent)
            {
                if (serviceState.GetSiPMDataByDarkCurrentID(e.Data.Identifier, out c))
                {
                    c.DarkCurrentResult.LeakageCurrentResult = e.Data;
                    FileOperationHelper.SaveIVResult(c, outputBaseDir); //it will overwrite everything
                    if (taskTypeWaitingForFinish == TaskTypes.DarkCurrent)
                    {
                        if (e.Data.ErrorHappened)
                        {
                            c.Checks.IDarkDone = true;
                            c.Checks.IDarkOK = false;
                            _logger.LogError($"{e.Data.ErrorMessage}");
                            CreateAndSendLogMessage("Failed VI measurement", $"VI measurement (LC) for {c.SiPMLocation.Block}, {c.SiPMLocation.Module}, {c.SiPMLocation.Array}, {c.SiPMLocation.SiPM} is failed. Reason: {c.IVResult.ErrorMessage}. Choose the next step!", LogMessageType.Error, Devices.NIMachine, true, ResponseButtons.StopRetryContinue, MeasurementType.DarkCurrentMeasurement);
                        }
                        else
                        {
                            c.Checks.IDarkDone = true;
                            try
                            {
                                //if (c.DarkCurrentResult.FirstDarkCurrentCompensated < 5E-7 && c.DarkCurrentResult.FirstDarkCurrentCompensated > -1E-8)
                                if (!c.DarkCurrentResult.FirstDarkCurrentCompensated.IsOutOfBoundaries(-1E-8, 5E-7))
                                {
                                    c.Checks.IDarkOK = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogInformation("Dark Current has a null value. Probably one of its measurements is not available yet.");
                            }
                            
                            taskIsRunning = false;
                            MarkCurrentTaskDone();
                        }
                        _hubContext.Clients.All.ReceiveSiPMChecksChange(c.SiPMLocation, c.Checks);
                    }
                }
                else
                {
                    _logger.LogError($"Unknown measurement identifier on DarkCurrentMeasurement LC");
                }
                
            }
            else if (e.Data.StartModel.MeasurementType == VoltageAndCurrentMeasurementTypes.ForwardResistance)
            {
                if (serviceState.GetSiPMDataByForwardResistanceID(e.Data.Identifier, out c))
                {
                    double? res = resistanceValues.GetResistance(c.SiPMLocation.Block, c.SiPMLocation.Module, c.SiPMLocation.Array);
                    if (res != null)
                    {
                        c.ForwardResistanceResult.InstrumentResistance = (double)res;
                    }
                    c.ForwardResistanceResult.Result = e.Data;
                    c.ForwardResistanceResult.Result.Temperatures = Temperatures.Where(item => item.Timestamp >= c.ForwardResistanceResult.Result.StartTimestamp - Pulser.UpdatePeriod.TotalSeconds && item.Timestamp <= c.ForwardResistanceResult.Result.EndTimestamp).ToList();
                    FileOperationHelper.SaveIVResult(c, outputBaseDir); //it will overwrite everything
                    if (taskTypeWaitingForFinish == TaskTypes.ForwardResistance)
                    {
                        if (e.Data.ErrorHappened)
                        {
                            c.Checks.RForwardDone = true;
                            c.Checks.RForwardOK = false;
                            _logger.LogError($"{e.Data.ErrorMessage}");
                            CreateAndSendLogMessage("Failed VI measurement", $"VI measurement (FR) for {c.SiPMLocation.Block}, {c.SiPMLocation.Module}, {c.SiPMLocation.Array}, {c.SiPMLocation.SiPM} is failed. Reason: {c.IVResult.ErrorMessage}. Choose the next step!", LogMessageType.Error, Devices.NIMachine, true, ResponseButtons.StopRetryContinue, MeasurementType.ForwardResistanceMeasurement);
                        }
                        else
                        {
                            c.Checks.RForwardDone = true;
                            try
                            {
                                if (c.ForwardResistanceResult.ForwardResistance.IsBetweenLimits(22.5, 2.5))
                                {
                                    c.Checks.RForwardOK = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogInformation($"Forward resistance error: {ex.Message}");
                            }
                            taskIsRunning = false;
                            MarkCurrentTaskDone();
                        }
                        _hubContext.Clients.All.ReceiveSiPMChecksChange(c.SiPMLocation, c.Checks);
                    }
                }
                else
                {
                    _logger.LogError($"Unknown measurement identifier on ForwardResistanceMeasurement");
                }
                
            }
            else
            {
                _logger.LogError($"Unknown type of VI measurement");
                CreateAndSendLogMessage("Failed VI measurement", $"Unknown type of VI measurement received: {e.Data.StartModel.MeasurementType}", LogMessageType.Warning, Devices.NIMachine, false, ResponseButtons.OK, MeasurementType.DarkCurrentMeasurement);

            }


            //_hubContext.Clients.All.ReceiveSiPMForwarMeasurementDataUpdate(c.SiPMLocation);
            _logger.LogInformation("CheckNext on VIMeasurementReceived");
            CheckAndRunNextAsync();
            _logger.LogInformation("CheckNext called on VIMeasurementReceived");
        }

        private void RefreshAPSUStates()
        {
            try
            {
                APSUDiagResponse diagResp = Pulser.GetAPSUStates();
                //string psuStates = JsonConvert.SerializeObject(diagResp);
                //CreateAndSendLogMessage("PSU States", psuStates, LogMessageType.Info, Devices.APSU, false, ResponseButtons.OK, MeasurementType.Unknown);
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
                    Pulser.ComReset(); //disable all coolers and psus
                    Thread.Sleep(5000);
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
                //_logger.LogError($"Invalid state of Pulser: {e.ReceivedMessage}");
                return;
            }
            if (e.ReceivedMessage.Contains("A_PSU_disabled!!", StringComparison.InvariantCultureIgnoreCase))
            {
                CreateAndSendLogMessage("Measurement Service - Pulser", "A_PSU is disabled. Trying to enable...", LogMessageType.Info, Devices.Pulser, false, ResponseButtons.OK, MeasurementType.Unknown);
                //_logger.LogError($"A_PSU is disabled. Trying to enable...");
                //if (CurrentTask == TaskTypes.IV)
                //{
                //    SetRetryFailedMeasurement(MeasurementType.IVMeasurement);
                //}
                CheckAndRunNextAsync();
            }

            else
            {
                CreateAndSendLogMessage("Measurement Service - Pulser", $"{e.ReceivedMessage}", LogMessageType.Error, Devices.Pulser, true, ResponseButtons.StopRetryContinue, MeasurementType.Unknown);
                //_logger.LogError($"A_PSU is disabled. Trying to enable...");
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
                //_logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                CreateAndSendLogMessage("Measurement Service - Init - HVPSU", ex.Message + " - Do you want to retry?", LogMessageType.Error, Devices.HVPSU, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                //_logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
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
            BTLDBSettings databaseSettings;

            try
            {
                MeasurementServiceSettings = new MeasurementServiceSettings(Configuration);
                ivMeasurementSettings = new IVMeasurementSettings(Configuration);
                dmmMeasurementSettings = new DMMMeasurementSettings(Configuration);

                niSettings = new NIMachineSettings(configuration);
                pulserSettings = new SerialSettings(configuration, "Pulser");
                hvpsuSeettings = new SerialSettings(configuration, "HVPSU");

                darkCurrentConfig = new DarkCurrentConfig();
                configuration.GetSection("DefaultMeasurementSettings:DarkCurrentConfig").Bind(darkCurrentConfig);

                forwardResistanceConfig = new ForwardResistanceConfig();
                configuration.GetSection("DefaultMeasurementSettings:ForwardResistanceConfig").Bind(forwardResistanceConfig);

                databaseSettings = new BTLDBSettings();
                configuration.GetSection("Database").Bind(databaseSettings);

                databaseHandler = new BTLDBHandler();
                databaseHandler.ApplySettings(databaseSettings);

                serviceState = new ServiceStateHandler(MeasurementServiceSettings.BlockCount, MeasurementServiceSettings.ModuleCount,
                                                        MeasurementServiceSettings.ArrayCount, MeasurementServiceSettings.SiPMCount);

                serviceState.OnActiveSiPMsChanged += ServiceState_OnActiveSiPMsChanged;

                FillEmptyCurrentRun(MeasurementServiceSettings.BlockCount, MeasurementServiceSettings.ModuleCount,
                                    MeasurementServiceSettings.ArrayCount, MeasurementServiceSettings.SiPMCount);

                resistanceValues = new ResistanceCompensationValues(configuration);

                exportConfig = new ExportConfig(configuration);
                currentExportPath = exportConfig.BasePath;
                _logger.LogInformation($"Export path set to \'{currentExportPath}\'");
            }
            catch (Exception ex)
            {
                CreateAndSendLogMessage("Measurement Service - Settings", ex.Message, LogMessageType.Error, Devices.Unknown, false, ResponseButtons.OK, MeasurementType.Unknown);
                //_logger.LogError($"Measurement servicse is unavailable because of an error: {ex.Message}");
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

            coolerState = new CoolerStateHandler(MeasurementServiceSettings);
            coolerState.OnCoolerTemperatureStabilizationChanged += CoolerState_OnCoolerTemperatureStabilizationChanged;
            coolerState.OnCoolerFail += CoolerState_OnCoolerFail;

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

        private void CoolerState_OnCoolerFail(object? sender, CoolerFailEventArgs e)
        {
            CreateAndSendLogMessage("Cooler Error",
                $"Block {e.Block}, Module {e.Module} cooler failed. Error: {e.CurrentState}. Check and manually restart cooler from Pulser state - Details, measurement will continue when temperature stabilised.",
                LogMessageType.Warning,
                Devices.Pulser,
                true,
                ResponseButtons.OK,
                MeasurementType.Unknown);
        }

        private void ServiceState_OnActiveSiPMsChanged(object? sender, ActiveSiPMsChangedEventArgs e)
        {
            _hubContext.Clients.All.ReceiveActiveSiPMs(e.ActiveSiPMs);
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
        }

        private void OnDMMMeasurementDataReceived(object? sender, DMMMeasurementDataReceivedEventArgs e)
        {
            _logger.LogInformation("DMMMeasurementReceived");
            if (taskTypeWaitingForFinish == TaskTypes.DMMResistance)
            {
                taskIsRunning = false;
                if (e.Data.ErrorHappened) // ask user
                {
                    CreateAndSendLogMessage("Failed DMM measurement", $"DMM resistance measurement for Block: {actualBlock}, Module: {actualModule} is failed. Reason: {e.Data.ErrorMessage}. Do you want to retry?", LogMessageType.Error, Devices.NIMachine, true, ResponseButtons.YesNo, MeasurementType.DMMResistanceMeasurement);
                }
                else // continue automatically if done properly
                {
                    //if (Math.Abs(MeasurementServiceSettings.ExpectedDMMResistance - e.Data.Resistance) >= MeasurementServiceSettings.DMMResistanceMaxDifference)
                    if (!e.Data.Resistance.IsBetweenLimits(MeasurementServiceSettings.ExpectedDMMResistance, MeasurementServiceSettings.DMMResistanceMaxDifference))
                    {
                        CreateAndSendLogMessage("Failed DMM measurement", $"DMM resistance ({e.Data.Resistance}) measurement for Block: {actualBlock} is out of range. Skipping this block", LogMessageType.Error, Devices.NIMachine, false, ResponseButtons.OK, MeasurementType.DMMResistanceMeasurement);
                        SkipCurrentBlock();
                    }
                    else
                    {
                        MarkCurrentTaskDone();
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
                    CheckAndRunNextAsync();
                }
                
            }          
        }

        private void AnalyseSiPM(CurrentSiPMModel sipm)
        {
            CurrentMeasurementDataModel c = serviceState.GetSiPMMeasurementData(sipm.Block, sipm.Module, sipm.Array, sipm.SiPM);

            if (c.Temperatures != null && c.Temperatures.Count > 0)
            {
                _logger.LogInformation("Temperatures list on IVMeasurementDataReceived has at least one measurement");
                c.IVResult.Temperatures = c.Temperatures; //use directly measured temperatures
            }
            else
            {
                c.IVResult.Temperatures = Temperatures.Where(item => item.Timestamp >= c.IVResult.StartTimestamp && item.Timestamp <= c.IVResult.EndTimestamp && item.Block <= c.SiPMLocation.Block).ToList();
                _logger.LogInformation("Got temperatures list from automatic updates");
            }

            CoolerSettingsModel cooler = coolerState.GetCoolerSettings(c.SiPMLocation.Block, c.SiPMLocation.Module);
            double tempSet = cooler.TargetTemperature;
            bool allTempAroundSet = true;

            for (int i = 0; i < c.IVResult.Temperatures.Count; i++)
            {
                double[] tempArray = (c.SiPMLocation.Module == 0 ? c.IVResult.Temperatures[i].Module1 : c.IVResult.Temperatures[i].Module2);
                for (int j = 0; j < tempArray.Length; j++)
                {
                    //if (Math.Abs(tempArray[j] - 25) > 3)
                    if (!tempArray[j].IsBetweenLimits(tempSet, MeasurementServiceSettings.TemperatureMaxDifference))
                    {
                        allTempAroundSet = false;
                        break;
                    }
                }
            }

            if (c.IVResult.Temperatures.Count >= 2 && allTempAroundSet)
            {
                c.Checks.IVTemperatureOK = true;
            }

            //append latest dmm resistance measurement if available
            c.DMMResistanceResult = serviceState.DMMResistances.LastOrDefault(new DMMResistanceMeasurementResponseModel());

            if (c.DMMResistanceResult.Resistance.IsBetweenLimits(MeasurementServiceSettings.ExpectedDMMResistance, MeasurementServiceSettings.DMMResistanceMaxDifference))
            {
                c.Checks.DMMResistanceOK = true;
            }

            //Run analysis and save data there even if analysis fails
            RunAnalysis(c); //async call

            if (taskTypeWaitingForFinish == TaskTypes.Analysis)
            {
                taskIsRunning = false;
                MarkCurrentTaskDone();
            }
            CheckAndRunNextAsync();
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

                c.Checks.IVDone = true;
            }
            else if (e.Data.ErrorHappened)
            {
                c.Checks.IVMeasurementOK = false;
                CreateAndSendLogMessage("Measurement not data not found", $"Unknown measurement's data received.", LogMessageType.Warning, Devices.NIMachine, false, ResponseButtons.OK, MeasurementType.IVMeasurement);
                return;
            }
            else
            {
                _logger.LogError($"Unknown measurement ID ({e.Data.Identifier})");
                return;
            }

            if (c.IVResult.ErrorHappened)
            {
                c.Checks.IVMeasurementOK = false;
                CreateAndSendLogMessage("Failed IV measurement", $"IV measurement for {c.SiPMLocation.Block}, {c.SiPMLocation.Module}, {c.SiPMLocation.Array}, {c.SiPMLocation.SiPM} is failed. Reason: {c.IVResult.ErrorMessage}. Do you want to retry?", LogMessageType.Error, Devices.NIMachine, true, ResponseButtons.YesNo, MeasurementType.IVMeasurement);
                return;
            }
            else
            {
                c.Checks.IVMeasurementOK = true;
            }

            _hubContext.Clients.All.ReceiveSiPMIVMeasurementDataUpdate(c.SiPMLocation);
            _hubContext.Clients.All.ReceiveSiPMChecksChange(c.SiPMLocation, c.Checks);

            if (taskTypeWaitingForFinish == TaskTypes.IV)
            {
                taskIsRunning = false;
                MarkCurrentTaskDone();
            }
            CheckAndRunNextAsync();
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
                        else
                        {
                            log.NextStep = ErrorNextStep.Retry;
                        }
                        break;
                    case ResponseButtons.No:
                        if (log.Sender.ToLower().Contains("Init".ToLower()))
                        {
                            log.NextStep = ErrorNextStep.Stop;
                        }
                        else
                        {
                            log.NextStep = ErrorNextStep.Continue;
                        }
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
                    taskIsRunning = false;
                    MarkCurrentTaskDone();
                    CheckAndRunNextAsync();
                    break;
                case ErrorNextStep.Retry:
                    //SetRetryFailedMeasurement(log.MeasurementType);
                    taskIsRunning = false;
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

        public static string GetHomePath(string relativePath)
        {
            // Get the user's home directory
            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Replace the ~/ prefix with the home directory path
            if (relativePath.StartsWith("~/"))
            {
                return Path.Combine(homeDirectory, relativePath.Substring(2));
            }

            return relativePath;
        }

        public List<string> GetListOfDirs(string path)
        {
            List<string> dirs;

            bool startsWith = path.StartsWith(exportConfig.BasePath);
            bool containsUpperLevel = path.Contains("..");
            bool exists = Directory.Exists(path);
            var enumerationOptions = new EnumerationOptions();
            enumerationOptions.IgnoreInaccessible = true;
            enumerationOptions.ReturnSpecialDirectories = false;
            enumerationOptions.AttributesToSkip = FileAttributes.Hidden;
            if (startsWith && !containsUpperLevel && exists)
            {
                //dirs = Directory.EnumerateDirectories(path).ToList();
                dirs = Directory.EnumerateDirectories(path, "*", enumerationOptions)
                    .Select(p => p.Replace(path, "")).ToList();
                dirs.RemoveAll(str => str.Contains("/."));
            }
            else
            {
                dirs = new List<string>();
            }
            return dirs;
        }

        public bool SetDir(string dir)
        {
            if (dir.StartsWith(exportConfig.BasePath) && !dir.Contains("..") && Directory.Exists(dir))
            {
                currentExportPath = dir;
                return true;
            }

            return false;
        }

        public string GetCurrentExportDir()
        {
            return currentExportPath;
        }

        public List<CurrentSiPMModel> ExportSiPMsData(ExportSiPMList list)
        {
            return serviceState.ExportSiPMsData(list, currentExportPath);
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
            if (s.Enabled == coolerState.GetCoolerSettings(s.Block, s.Module).Enabled)
            {
                //already in same state
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

