using System;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
    public class NIMachineSettings
    {
        public bool Enabled { get; set; } = false;
        public string IP { get; set; } = "127.0.0.1";
        public int CommandPort { get; set; } = 0;
        public int LogPort { get; set; } = 0;
        public int UpdatePeriodms { get; set; } = 0;

        public NIMachineSettings(IConfiguration config)
        {
            var niMachineEnabled = config["NIMachine:Enabled"];
            var ip = config["NIMachine:IP"];
            var commandPort = config["NIMachine:CommandPort"];
            var logPort = config["NIMachine:CommandPort"];
            var updatePeriodms = config["NIMachine:updatePeriodms"];

            if (niMachineEnabled != null)
            {
                bool val;
                bool.TryParse(niMachineEnabled, out val);
                Enabled = val;
            }
            if (ip != null)
            {
                IP = ip;
            }
            if (commandPort != null)
            {
                int val;
                int.TryParse(commandPort, out val);
                CommandPort = val;
            }
            if (logPort != null)
            {
                int val;
                int.TryParse(logPort, out val);
                LogPort = val;
            }
            if (updatePeriodms != null)
            {
                int val;
                int.TryParse(updatePeriodms, out val);
                UpdatePeriodms = val;
            }
        }
    }

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

        public NIMachine(IConfiguration config, ILogger<NIMachine> logger) : this(new NIMachineSettings(config), logger)
        {

        }

        public NIMachine(NIMachineSettings settings, ILogger<NIMachine> logger) : this(settings.Enabled, settings.IP, settings.CommandPort, settings.LogPort, TimeSpan.FromMilliseconds(settings.UpdatePeriodms), logger)
        {
        }

		public NIMachine(bool enabled, string ip, int controlPort, int logPort, TimeSpan period, ILogger<NIMachine> logger) :
			base("NIMachine", ip, controlPort, logPort, period, logger)
		{
            if (enabled)
            {
                base.Start();
            }
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
            bool success = AskServer("GetCurrentSiPM", out received);
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
