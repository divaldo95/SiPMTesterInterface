﻿using System;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.ClientApp.Services
{
	public class MeasurementService
	{
        private readonly object _lockObject = new object();

        private ConnectionState _IVConnectionState;
        private ConnectionState _SPSConnectionState;
        private MeasurementState _IVState;
        private MeasurementState _SPSState;

        public MeasurementService()
        {
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

