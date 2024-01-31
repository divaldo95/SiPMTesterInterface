using System;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
	public class NIMachine : SiPMInstrument
	{
        private object _lockObject = new object();
        private CurrentSiPMModel _currentSiPM = new CurrentSiPMModel();

        public CurrentSiPMModel SiPMUnderTest
        {
            get
            {
                lock(_lockObject)
                {
                    return _currentSiPM;
                }
                
            }
            private set
            {
                _currentSiPM = value;
            }
        }

        public IVModel GetCurrentState()
        {
            IVModel state = new IVModel();
            state.ConnectionState = ConnectionState;
            state.MeasurementState = MeasurementState;
            state.CurrentVoltage = 0.1;
            state.CurrentSiPM = SiPMUnderTest;
            return state;
        }

		public NIMachine(string InstrumentName, string ip, int controlPort, int logPort, TimeSpan period, ILogger<NIMachine> logger) :
			base(InstrumentName, ip, controlPort, logPort, period, logger)
		{
		}

		protected override void HandleMessage(string message)
		{

		}

        //Must be quick, the timer calls this function periodically
        protected override void PeriodicUpdate()
        {
            SiPMUnderTest = GetSiPMUnderTest();
        }

        public CurrentSiPMModel GetSiPMUnderTest()
		{
            string received = "";
            bool success = AskServer("GetState", out received);
            if (success)
            {
                return new CurrentSiPMModel(received);
            }
            else
            {
                return new CurrentSiPMModel();
            }
        }
	}
}
