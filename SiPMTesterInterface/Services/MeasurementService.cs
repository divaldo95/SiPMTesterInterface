using System;
using Microsoft.AspNetCore.SignalR;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Hubs;
using SiPMTesterInterface.Interfaces;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.ClientApp.Services
{
    public class MeasurementService
	{
        private readonly object _lockObject = new object();

        private readonly NIMachine niMachine;

        private GlobalStateModel globalState = new GlobalStateModel();

        private readonly IConfiguration Configuration;

        private readonly IHubContext<UpdatesHub, IStateContext> _hubContext;

        private ConnectionState _IVConnectionState;
        private ConnectionState _SPSConnectionState;
        private MeasurementState _IVState;
        private MeasurementState _SPSState;

        private readonly ILogger<MeasurementService> _logger;

        private void OnIVConnectionStateChangeCallback(object? sender, ConnectionStateChangedEventArgs e)
        {
            _IVConnectionState = e.State;
        }

        private void OnIVMeasurementStateChangeCallback(object? sender, MeasurementStateChangedEventArgs e)
        {
            _IVState = e.State;
            Console.WriteLine("IV changed");
            _hubContext.Clients.All.ReceiveIVMeasurementStateChange(_IVState);

        }

        public MeasurementService(ILogger<MeasurementService> logger, ILogger<NIMachine> niLogger, IHubContext<UpdatesHub, IStateContext> hubContext, IConfiguration configuration)
        {
            Configuration = configuration;
            _logger = logger;
            _hubContext = hubContext;
            //Console.WriteLine("Look for configuration here:");

            //Console.WriteLine(ip);

            niMachine = new NIMachine(configuration, niLogger);
            niMachine.OnConnectionStateChanged += OnIVConnectionStateChangeCallback;
            niMachine.OnMeasurementStateChanged += OnIVMeasurementStateChangeCallback;

            // Set initial states
            _IVState = MeasurementState.NotRunning;
            _SPSState = MeasurementState.NotRunning;

            _IVConnectionState = ConnectionState.Disconnected;
            _SPSConnectionState = ConnectionState.Disconnected;
        }

        public MeasurementState IVState
        {
            get
            {
                lock (_lockObject)
                {
                    return _IVState;
                }
            }
        }

        public MeasurementState SPSState
        {
            get
            {
                lock (_lockObject)
                {
                    return _SPSState;
                }
            }
        }

        public ConnectionState IVConnectionState
        {
            get
            {
                lock (_lockObject)
                {
                    return _IVConnectionState;
                }
            }
        }

        public ConnectionState SPSConnectionState
        {
            get
            {
                lock (_lockObject)
                {
                    return _SPSConnectionState;
                }
            }
        }

        public void UpdateIVState(MeasurementState s)
        {
            lock (_lockObject)
            {
                _IVState = s;
            }
        }

        public void UpdateSPSState(MeasurementState s)
        {
            lock (_lockObject)
            {
                _SPSState = s;
            }
        }

        public void UpdateIVConnectionState(ConnectionState c)
        {
            lock (_lockObject)
            {
                _IVConnectionState = c;
            }
        }

        public void UpdateSPSConnectionState(ConnectionState c)
        {
            lock (_lockObject)
            {
                _SPSConnectionState = c;
            }
        }
    }
}

