using System;
using System.Collections.Generic;
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
        public double CurrentLimit { get; set; } = 0.01;
        public double CurrentLimitRange { get; set; } = 0.01;
        public int LED { get; set; } = 100;

        public IVMeasurementSettings(IConfiguration config)
        {
            var currentLimit = config["DefaultMeasurementSettings:IV:CurrentLimit"];
            var currentLimitRange = config["DefaultMeasurementSettings:IV:CurrentLimitRange"];
            var led = config["DefaultMeasurementSettings:IV:LED"];

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

        private ServiceStateHandler serviceState;
        private CoolerStateHandler coolerState;
        private List<CurrentSiPMModel> ivSiPMs;
        private List<CurrentSiPMModel> spsSiPMs;

        private bool _PSUAEnabled = false;

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
            if (!DeviceStates.IsAllEnabledDeviceStarted())
            {
                CreateAndSendLogMessage("PopulateServiceState", "MeasurementService has devices which are not started", LogMessageType.Error, false, ResponseButtons.OK, MeasurementType.Unknown);
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

        public LogMessageModel CreateAndSendLogMessage(string sender, string message, LogMessageType logType, bool interactionNeeded, ResponseButtons buttons, MeasurementType type = MeasurementType.Unknown)
        {
            LogMessageModel logMessage;
            logMessage = new LogMessageModel(sender, message, logType, type, interactionNeeded, buttons);
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

            if (Pulser.Enabled && !_PSUAEnabled)
            {
                try
                {
                    double voltage = 0.0;
                    double current = 0.0;
                    Pulser.EnablePSU(0, PSUs.PSU_A, out voltage, out current);
                    _PSUAEnabled = true;
                    CreateAndSendLogMessage("Measurement Service - APSU", $"PSU A enabled. Voltage: {voltage}, Current: {current}", LogMessageType.Info, false, ResponseButtons.OK, MeasurementType.Unknown);
                }
                catch (Exception ex)
                {
                    CreateAndSendLogMessage("Measurement Service - Check and Run next - APSU", ex.Message + " - Do you want to retry?", LogMessageType.Error, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                    _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
                }
                
            }

            var logList = GetAttentionNeededLogs();

            if (logList.Count > 0)
            {
                _logger.LogWarning($"{logList.Count} logs waiting for user response");
                return;
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
                        Thread.Sleep(200);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error on DMM CheckAndRunNext(): {ex.Message}");
                        globalState.GlobalIVMeasurementState = MeasurementState.Error;
                        CreateAndSendLogMessage("CheckAndRunNext - DMM Resistance", ex.Message, LogMessageType.Error, true, ResponseButtons.StopRetryContinue, MeasurementType.DMMResistanceMeasurement);
                        return;
                    }
                    niMachine?.StartDMMResistanceMeasurement(niDMMStart);
                }

                else if (Type == MeasurementType.IVMeasurement)
                {
                    CurrentTask = TaskTypes.IV;
                    ivSiPMs = sipms;
                    NIIVStartModel niIVStart = nextMeasurementData as NIIVStartModel; //some settings duplicated here and in ServiceState
                    if (sipms.Count != 1)
                    {
                        CreateAndSendLogMessage("CheckAndRunNext - IV", "Can not measure more than one SiPM at a time for IV", LogMessageType.Error, false, ResponseButtons.OK, MeasurementType.IVMeasurement);
                        _logger.LogError("Can not measure more than one SiPM at a time for IV");
                        return;
                    }
                    //TODO: Get the right led pulser values here
                    //set IV settings
                    //change SiPM relays
                    int[] pulserLEDValues = new[]
                    {
                        LEDValueHelper.GetPulserValue(ivMeasurementSettings.LED),
                        LEDValueHelper.GetPulserValue(ivMeasurementSettings.LED),
                        LEDValueHelper.GetPulserValue(ivMeasurementSettings.LED),
                        LEDValueHelper.GetPulserValue(ivMeasurementSettings.LED),
                    };
                    try
                    {
                        Pulser?.SetMode(ivSiPMs[0].Block, ivSiPMs[0].Module, ivSiPMs[0].Array, ivSiPMs[0].SiPM, MeasurementMode.MeasurementModes.IV,
                            new[] { pulserLEDValues[0], pulserLEDValues[1], pulserLEDValues[2], pulserLEDValues[3] });
                        Thread.Sleep(200);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error on IV CheckAndRunNext(): {ex.Message}");
                        globalState.GlobalIVMeasurementState = MeasurementState.Error;
                        CreateAndSendLogMessage("CheckAndRunNext - IV", ex.Message, LogMessageType.Error, true, ResponseButtons.StopRetryContinue, MeasurementType.IVMeasurement);
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
                try
                {
                    if (Pulser.Enabled && _PSUAEnabled)
                    {
                        Pulser?.SetMode(0, 0, 0, 0, MeasurementMode.MeasurementModes.Off, new[] { 0, 0, 0, 0 });
                        Pulser?.DisablePSU(0, PSUs.PSU_A);
                        _PSUAEnabled = false;
                        CreateAndSendLogMessage("Measurement Service - APSU", $"PSU A disabled.", LogMessageType.Info, false, ResponseButtons.OK, MeasurementType.Unknown);
                    }
                    CurrentTask = TaskTypes.Finished;
                    EndTimestamp = TimestampHelper.GetUTCTimestamp();
                }
                catch (Exception ex)
                {
                    CreateAndSendLogMessage("Measurement Service - Check and Run next - APSU", ex.Message + " - Do you want to retry?", LogMessageType.Error, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                    _logger.LogError($"Measurement service error: {ex.Message}");
                }
                
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
                    bool hasEnoughCurrent = false; 
                    for (int i = 0; i < data.IVResult.SMUCurrent.Count; i++)
                    {
                        if (data.IVResult.SMUCurrent[i] > 10E-6)
                        {
                            hasEnoughCurrent = true;
                        }
                    }
                    if (hasEnoughCurrent)
                    {
                        //RootIVAnalyser.Analyse(data);
                        data.IVResult.AnalysationResult.Analysed = true;
                        data.IVResult.AnalysationResult.IsOK = true;
                    }
                    else
                    {
                        data.IVResult.AnalysationResult.Analysed = true;
                        data.IVResult.AnalysationResult.IsOK = false;
                    }
                    //_hubContext.Clients.All.ReceiveIVAnalysationResult(data.SiPMLocation, data.IVResult.AnalysationResult);
                    _hubContext.Clients.All.ReceiveIVAnalysationResult(data.SiPMLocation, new IVMeasurementHubUpdate(data.IVResult.AnalysationResult, new IVTimes(data.IVResult.StartTimestamp, data.IVResult.EndTimestamp))); //send mesaurement update
                    _logger.LogInformation("Analysis task done");
                }
                catch (Exception ex)
                {
                    CreateAndSendLogMessage("RunAnalysis", ex.Message, LogMessageType.Error, false, ResponseButtons.OK, MeasurementType.IVMeasurement);
                    _logger.LogError($"{ex.Message}");
                }
                
            });
            return t;
        }

        public void StartMeasurement(MeasurementStartModel measurementData)
        {
            PrepareMeasurement(measurementData);
            PopulateServiceState(); //store measurements
            CreateAndSendLogMessage("StartMeasurement", "Measurement is starting...", LogMessageType.Info, false, ResponseButtons.OK, MeasurementType.Unknown);
            MeasurementStopped = false;
            CheckAndRunNextAsync();
            StartTimestamp = TimestampHelper.GetUTCTimestamp();
        }

        public void StopMeasurement()
        {
            MeasurementStopped = true;
            niMachine?.StopMeasurement();
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
                CreateAndSendLogMessage("Measurement Service - Init - NI Machine", ex.Message + " - Do you want to retry?", LogMessageType.Error, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
            }
        }

        private void InitPulser()
        {
            try
            {
                var psocSerialLogger = _loggerFactory.CreateLogger<SerialPortHandler>();
                SerialSettings pulserSettings = new SerialSettings(Configuration, "Pulser");

                if (pulserSettings.Enabled && pulserSettings.AutoDetect)
                {
                    string port = SerialPortHandler.GetAutoDetectedPort(psocSerialLogger, pulserSettings.BaudRate, pulserSettings.Timeout, pulserSettings.AutoDetectString, pulserSettings.AutoDetectExpectedAnswer);
                    pulserSettings.SerialPort = port;
                }

                if (Pulser == null) Pulser = new PSoCCommunicator(pulserSettings, psocSerialLogger);
                Pulser.OnSerialStateChanged += Pulser_OnSerialStateChanged;
                Pulser.OnDataReadout += Pulser_OnDataReadout;

                double voltage = 0.0;
                double current = 0.0;

                if (Pulser.Enabled)
                {
                    DeviceStates.SetEnabled(Devices.Pulser, true);
                    Pulser.Init();
                    DeviceStates.SetInitState(Devices.Pulser, true);
                    Pulser.Start();
                    Pulser.EnablePSU(0, PSUs.PSU_A, out voltage, out current);
                    CreateAndSendLogMessage("Measurement Service - Init - Pulser", $"PSU A enabled. Voltage: {voltage}, Current: {current}", LogMessageType.Info, false, ResponseButtons.OK, MeasurementType.Unknown);
                    _PSUAEnabled = true;
                    DeviceStates.SetStartedState(Devices.Pulser, true);
                }
                else
                {
                    DeviceStates.SetEnabled(Devices.Pulser, false);
                }
            }
            catch (Exception ex)
            {
                CreateAndSendLogMessage("Measurement Service - Init - Pulser", ex.Message + " - Do you want to retry?", LogMessageType.Error, true, ResponseButtons.YesNo, MeasurementType.Unknown);
                _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
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
            catch (Exception ex)
            {
                CreateAndSendLogMessage("Measurement Service - Init - HVPSU", ex.Message + " - Do you want to retry?", LogMessageType.Error, true, ResponseButtons.YesNo, MeasurementType.Unknown);
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
                CreateAndSendLogMessage("Measurement Service - Settings", ex.Message, LogMessageType.Error, false, ResponseButtons.OK, MeasurementType.Unknown);
                _logger.LogError($"Measurement servicse is unavailable because of an error: {ex.Message}");
            }

            DeviceStates = new DeviceStatesModel();

            coolerState = new CoolerStateHandler();

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
            //don't know the analysis result yet
            FileOperationHelper.SaveIVResult(c, utcDate.ToString("yyyyMMddHHmmss"));

            RunAnalysis(c); //async call

            _hubContext.Clients.All.ReceiveSiPMIVMeasurementDataUpdate(c.SiPMLocation);

            CheckAndRunNext();
        }

        private void Pulser_OnDataReadout(object? sender, PSoCCommuicatorDataReadEventArgs e)
        {
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
                            if (log.Sender.ToLower().Contains("NI Machine".ToLower()))
                            {
                                log.NextStep = ErrorNextStep.ReInitNI;
                            }
                            else if (log.Sender.ToLower().Contains("Pulser".ToLower()))
                            {
                                log.NextStep = ErrorNextStep.ReInitPulser;
                            }
                            else if (log.Sender.ToLower().Contains("HVPSU".ToLower()))
                            {
                                log.NextStep = ErrorNextStep.ReInitHVPSU;
                            }
                            else if (log.Sender.ToLower().Contains("DAQ".ToLower()))
                            {
                                log.NextStep = ErrorNextStep.ReInitDAQ;
                            }
                            else if (log.Sender.ToLower().Contains("APSU".ToLower()))
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

        public List<Cooler> CoolerStates
        {
            get
            {
                if (Pulser != null)
                {
                    return Pulser.CoolerStates.ToList();
                }
                else
                {
                    return new List<Cooler>();
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

        public void SetCooler(CoolerSettingsModel s)
        {
            if (Pulser != null && Pulser.Enabled)
            {
                Pulser.SetCooler(s.Block, s.Module, s.Enabled, s.TargetTemperature, s.FanSpeed);
                coolerState.SetCoolerSettings(s);
            }
            else
            {
                throw new NullReferenceException("Cooler may be disabled or not available");
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

