﻿using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;
using SiPMTesterInterface.Hubs;
using SiPMTesterInterface.Interfaces;
using SiPMTesterInterface.Libraries;
using SiPMTesterInterface.Models;

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
        private readonly NIMachine niMachine;
        private readonly PSoCCommunicator Pulser;
        private readonly HVPSU hvPSU;

        public MeasurementServiceSettings MeasurementServiceSettings { get; private set; }
        public IVMeasurementSettings ivMeasurementSettings { get; private set; }
        public DMMMeasurementSettings dmmMeasurementSettings { get; private set; }

        private ServiceStateHandler serviceState;
        private List<CurrentSiPMModel> ivSiPMs;
        //private List<CurrentSiPMModel> spsSiPMs;

        private readonly IConfiguration Configuration;

        private readonly IHubContext<UpdatesHub, IStateContext> _hubContext;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MeasurementService> _logger;

        public long StartTimestamp { get; private set; } = 0;
        public long EndTimestamp { get; private set; } = 0;

        private DateTime utcDate;

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

        private void PopulateServiceState()
        {

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

        public void CheckAndRunNext()
        {
            MeasurementType Type;
            object nextMeasurementData;
            List<CurrentSiPMModel> sipms;

            //turn off pulser, disconnect all relays
            EndTimestamp = TimestampHelper.GetUTCTimestamp(); ; //just to make sure something is saved
            Console.WriteLine("Checking new iteration...");
            if (GetNextIterationData(out Type, out nextMeasurementData, out sipms))
            {
                Console.WriteLine("Found one");
                if (Type == MeasurementType.DMMResistanceMeasurement)
                {
                    NIDMMStartModel niDMMStart = nextMeasurementData as NIDMMStartModel;
                    niDMMStart.DMMResistance = globalState.CurrentRun.DMMResistance;
                    Pulser.SetMode(0, 0, 0, 0, MeasurementMode.MeasurementModes.DMMResistanceMeasurement, new[] { 0, 0, 0, 0 });
                    niMachine.StartDMMResistanceMeasurement(niDMMStart);
                }

                else if (Type == MeasurementType.IVMeasurement)
                {
                    ivSiPMs = sipms;
                    NIIVStartModel niIVStart = nextMeasurementData as NIIVStartModel; //some settings duplicated here and in ServiceState
                    if (sipms.Count != 1)
                    {
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
                    Pulser.SetMode(ivSiPMs[0].Block, ivSiPMs[0].Module, ivSiPMs[0].Array, ivSiPMs[0].SiPM, MeasurementMode.MeasurementModes.IV,
                        new[] { pulserLEDValues[0], pulserLEDValues[1], pulserLEDValues[2], pulserLEDValues[3] });
                    CurrentMeasurementDataModel c = serviceState.GetSiPMMeasurementData(ivSiPMs[0].Block, ivSiPMs[0].Module, ivSiPMs[0].Array, ivSiPMs[0].SiPM);
                    c.IVMeasurementID = niIVStart.Identifier;
                    niMachine.StartIVMeasurement(niIVStart);
                }
            }
            else //end of measurement
            {
                Pulser.SetMode(0, 0, 0, 0, MeasurementMode.MeasurementModes.Off, new[] { 0, 0, 0, 0 });
                EndTimestamp = TimestampHelper.GetUTCTimestamp();
            }
        }

        private Task RunAnalysis(CurrentMeasurementDataModel data)
        {
            Task t = new Task(() =>
            {
                try
                {
                    RootIVAnalyser.Analyse(data);
                    _hubContext.Clients.All.ReceiveIVAnalysationResult(data.SiPMLocation, data.IVResult.AnalysationResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{ex.Message}");
                }
                
            });
            return t;
        }

        public void StartMeasurement(MeasurementStartModel measurementData)
        {
            PrepareMeasurement(measurementData);
            PopulateServiceState(); //store measurements
            CheckAndRunNext();
            StartTimestamp = TimestampHelper.GetUTCTimestamp();
        }

        public void StopMeasurement()
        {
            niMachine.StopMeasurement();
        }

        private void OnDMMMeasurementDataReceived(object? sender, DMMMeasurementDataReceivedEventArgs e)
        {
            //save data here
            serviceState.AppendDMMResistanceMeasurement(e.Data);

            _logger.LogInformation("DMMMeasurementReceived");
            
            CheckAndRunNext();
        }

        private void OnIVMeasurementDataReceived(object? sender, IVMeasurementDataReceivedEventArgs e)
        {
            //save data here

            _logger.LogInformation("IVMeasurementReceived");
            CurrentMeasurementDataModel c;
            if (serviceState.GetSiPMIVMeasurementData(e.Data.Identifier, out c))
            {
                c.IVResult = e.Data;
                c.IsIVDone = true;
            }
            c.IVResult.Temperatures = Temperatures.Where(item => item.Timestamp >= c.IVResult.StartTimestamp && item.Timestamp <= c.IVResult.EndTimestamp).ToList();
            //don't know the analysis result yet
            FileOperationHelper.SaveIVResult(c, utcDate.ToString("yyyyMMddHHmmss"));
            _hubContext.Clients.All.ReceiveSiPMIVMeasurementDataUpdate(c.SiPMLocation, new IVMeasurementHubUpdate(false, 0.0, e.Data.StartTimestamp, e.Data.EndTimestamp)); //send mesaurement update

            RunAnalysis(c); //async call

            CheckAndRunNext();
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
            if (Pulser != null)
            {
                Pulser.SetCooler(s.Block, s.Module, s.Enabled, s.TargetTemperature, s.FanSpeed);
                serviceState.SetCoolerSettings(s);
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
                return serviceState.GetCoolerSettings(block, module);
            }
            else
            {
                throw new NullReferenceException("Pulser may be disabled or not available");
            }
        }

        public MeasurementService(ILoggerFactory loggerFactory, IHubContext<UpdatesHub, IStateContext> hubContext, IConfiguration configuration) : base()
        {
            Configuration = configuration;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<MeasurementService>();
            _hubContext = hubContext;

            try
            {
                MeasurementServiceSettings = new MeasurementServiceSettings(Configuration);
                ivMeasurementSettings = new IVMeasurementSettings(Configuration);
                dmmMeasurementSettings = new DMMMeasurementSettings(Configuration);

                var niLogger = _loggerFactory.CreateLogger<NIMachine>();
                var psocSerialLogger = _loggerFactory.CreateLogger<SerialPortHandler>();
                var hvpsuSerialLogger = _loggerFactory.CreateLogger<SerialPortHandler>();

                niMachine = new NIMachine(Configuration, niLogger);
                niMachine.OnConnectionStateChanged += OnIVConnectionStateChangeCallback;
                niMachine.OnMeasurementStateChanged += OnIVMeasurementStateChangeCallback;

                niMachine.OnIVMeasurementDataReceived += OnIVMeasurementDataReceived;
                niMachine.OnDMMMeasurementDataReceived += OnDMMMeasurementDataReceived;

                Pulser = new PSoCCommunicator(Configuration, psocSerialLogger);
                Pulser.OnSerialStateChanged += Pulser_OnSerialStateChanged;
                Pulser.OnDataReadout += Pulser_OnDataReadout;
                hvPSU = new HVPSU(Configuration, hvpsuSerialLogger);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Measurement service is unavailable because of an error: {ex.Message}");
            }



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

        private void Pulser_OnDataReadout(object? sender, PSoCCommuicatorDataReadEventArgs e)
        {
            _hubContext.Clients.All.ReceivePulserTempCoolerData(e);
        }

        private void Pulser_OnSerialStateChanged(object? sender, SerialConnectionStateChangedEventArgs e)
        {
            _hubContext.Clients.All.ReceivePulserStateChange(e);
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
    }
}

