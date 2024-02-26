using System;
using Microsoft.AspNetCore.SignalR;
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

        private readonly MeasurementOrchestrator orchestrator;

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

        public void StartMeasurement(MeasurementStartModel measurementData)
        {
            orchestrator.PrepareMeasurement(measurementData);
            
            // Run DMM resistance measurement if any IV measurement is set
            if (IsIVIterationAvailable())
            {
                if (measurementData.MeasureDMMResistance)
                {
                    niMachine.StartDMMResistanceMeasurement(measurementData.DMMResistance); //start with dmm resistance measurement if enabled
                }
                else
                {
                    NIMachineStartModel startModel;
                    if (GetNextIterationData(out startModel))
                    {
                        niMachine.StartIVMeasurement(startModel);
                    }
                    else
                    {
                        _logger.LogError("Error while getting the next iteration on StartMeasurement");
                    }
                }
            }

        }

        private void OnDMMMeasurementDataReceived(object? sender, DMMMeasurementDataReceivedEventArgs e)
        {
            //save data here

            NIMachineStartModel startModel;
            if (GetNextIterationData(out startModel))
            {
                niMachine.StartIVMeasurement(startModel);
            }
        }

        private void OnIVMeasurementDataReceived(object? sender, IVMeasurementDataReceivedEventArgs e)
        {
            //save data here

            NIMachineStartModel startModel;
            if (GetNextIterationData(out startModel))
            {
                niMachine.StartIVMeasurement(startModel);
            }
        }

        public MeasurementService(ILogger<MeasurementService> logger, ILogger<NIMachine> niLogger, IHubContext<UpdatesHub, IStateContext> hubContext, IConfiguration configuration) : base()
        {
            Configuration = configuration;
            _logger = logger;
            _hubContext = hubContext;

            orchestrator = new MeasurementOrchestrator();

            niMachine = new NIMachine(Configuration, niLogger);
            niMachine.OnConnectionStateChanged += OnIVConnectionStateChangeCallback;
            niMachine.OnMeasurementStateChanged += OnIVMeasurementStateChangeCallback;

            niMachine.OnIVMeasurementDataReceived += OnIVMeasurementDataReceived;
            niMachine.OnDMMMeasurementDataReceived += OnDMMMeasurementDataReceived;

            Pulser = new PSoCCommunicator(Configuration);
            hvPSU = new HVPSU(Configuration);
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

