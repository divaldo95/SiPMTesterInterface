using System;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Hubs;
using SiPMTesterInterface.Interfaces;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.ClientApp.Services
{
    public class MeasurementService : MeasurementOrchestrator
    {
        private readonly object _lockObject = new object();

        private bool MeasStateChanged = false;
        private bool MeasDataReceived = false;

        //connected instruments
        private readonly NIMachine niMachine;
        private readonly PSoCCommunicator Pulser;
        private readonly HVPSU hvPSU;

        //private readonly MeasurementOrchestrator orchestrator;

        private readonly IConfiguration Configuration;

        private readonly IHubContext<UpdatesHub, IStateContext> _hubContext;

        private readonly ILogger<MeasurementService> _logger;

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
            if (GetNextIterationData(out Type, out nextMeasurementData, out sipms))
            {
                
                if (Type == MeasurementType.DMMResistanceMeasurement)
                {
                    NIDMMStartModel niDMMStart = nextMeasurementData as NIDMMStartModel;
                    niDMMStart.DMMResistance = globalState.CurrentRun.DMMResistance;
                    niMachine.StartDMMResistanceMeasurement(niDMMStart);
                }

                else if (Type == MeasurementType.IVMeasurement)
                {
                    NIIVStartModel niIVStart = nextMeasurementData as NIIVStartModel;
                    if (sipms.Count != 1)
                    {
                        _logger.LogError("Can not measure more than one SiPM at a time for IV");
                        return;
                    }
                    //set IV settings
                    //change SiPM relays
                    niMachine.StartIVMeasurement(niIVStart);
                }
            }
        }

        public void StartMeasurement(MeasurementStartModel measurementData)
        {
            PrepareMeasurement(measurementData);
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

            CheckAndRunNext();
        }

        public MeasurementService(ILogger<MeasurementService> logger, ILogger<NIMachine> niLogger, IHubContext<UpdatesHub, IStateContext> hubContext, IConfiguration configuration) : base()
        {
            Configuration = configuration;
            _logger = logger;
            _hubContext = hubContext;

            try
            {
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

