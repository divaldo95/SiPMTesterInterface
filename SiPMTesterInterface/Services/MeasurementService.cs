using System;
using System.ComponentModel;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Hubs;
using SiPMTesterInterface.Interfaces;
using SiPMTesterInterface.Models;
using static NetMQ.NetMQSelector;

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

    public class MeasurementService : MeasurementOrchestrator, IDisposable
    {
        private readonly object _lockObject = new object();

        private bool disposed = false;

        //connected instruments
        private readonly NIMachine niMachine;
        private readonly PSoCCommunicator Pulser;
        private readonly HVPSU hvPSU;

        public MeasurementServiceSettings MeasurementServiceSettings { get; private set; }

        private ServiceStateHandler serviceState;
        private List<CurrentSiPMModel> ivSiPMs;
        //private List<CurrentSiPMModel> spsSiPMs;

        private readonly IConfiguration Configuration;

        private readonly IHubContext<UpdatesHub, IStateContext> _hubContext;

        private readonly ILogger<MeasurementService> _logger;

        private void PopulateServiceState()
        {
            serviceState = new ServiceStateHandler(MeasurementServiceSettings.BlockCount, MeasurementServiceSettings.ModuleCount,
                                                        MeasurementServiceSettings.ArrayCount, MeasurementServiceSettings.SiPMCount);

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
            Pulser.SetMode(0, 0, 0, 0, MeasurementMode.MeasurementModes.Off, new [] { 0, 0, 0, 0});

            if (GetNextIterationData(out Type, out nextMeasurementData, out sipms))
            {
                
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
                    //set IV settings
                    //change SiPM relays
                    Pulser.SetMode(ivSiPMs[0].Block, ivSiPMs[0].Module, ivSiPMs[0].Array, ivSiPMs[0].SiPM, MeasurementMode.MeasurementModes.IV, new[] { 100, 100, 100, 100 });
                    CurrentMeasurementDataModel c = serviceState.GetSiPMMeasurementData(ivSiPMs[0].Block, ivSiPMs[0].Module, ivSiPMs[0].Array, ivSiPMs[0].SiPM);
                    c.IVMeasurementID = niIVStart.Identifier;
                    niMachine.StartIVMeasurement(niIVStart);
                }
            }
        }

        public void StartMeasurement(MeasurementStartModel measurementData)
        {
            PrepareMeasurement(measurementData);
            PopulateServiceState(); //store measurements
            CheckAndRunNext();
        }

        private void OnDMMMeasurementDataReceived(object? sender, DMMMeasurementDataReceivedEventArgs e)
        {
            //save data here
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

        public MeasurementService(ILogger<MeasurementService> logger, ILogger<NIMachine> niLogger, IHubContext<UpdatesHub, IStateContext> hubContext, IConfiguration configuration) : base()
        {
            Configuration = configuration;
            _logger = logger;
            _hubContext = hubContext;

            try
            {
                MeasurementServiceSettings = new MeasurementServiceSettings(Configuration);

                niMachine = new NIMachine(Configuration, niLogger);
                niMachine.OnConnectionStateChanged += OnIVConnectionStateChangeCallback;
                niMachine.OnMeasurementStateChanged += OnIVMeasurementStateChangeCallback;

                niMachine.OnIVMeasurementDataReceived += OnIVMeasurementDataReceived;
                niMachine.OnDMMMeasurementDataReceived += OnDMMMeasurementDataReceived;

                Pulser = new PSoCCommunicator(Configuration);
                hvPSU = new HVPSU(Configuration);
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

