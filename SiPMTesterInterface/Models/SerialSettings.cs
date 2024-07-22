using System;
namespace SiPMTesterInterface.Models
{
	public class SerialSettings
	{
        public bool Enabled { get; set; } = true;
        public bool AutoDetect { get; set; } = false;
        public bool Debug { get; set; } = false;
        public string AutoDetectString { get; set; } = "";
        public string AutoDetectExpectedAnswer { get; set; } = "";
        public string SerialPort { get; set; } = "";
        public int BaudRate { get; set; } = 115200;
        public int Timeout { get; set; } = 10000;

		public SerialSettings(IConfiguration config, string obj)
		{
            var enabled = config[obj + ":Enabled"];
            var autoDetect = config[obj + ":AutoDetect"];
            var debug = config[obj + ":Debug"];
            var autoDetectString = config[obj + ":AutoDetectString"];
            var autoDetectExpectedAnswer = config[obj + ":AutoDetectExpectedAnswer"];
            var sPort = config[obj+":SerialPort"];
            var baud = config[obj + ":BaudRate"];
            var timeout = config[obj + ":Timeout"];

            if (enabled != null)
            {
                bool val;
                bool.TryParse(enabled, out val);
                Enabled = val;
            }
            if (autoDetect != null)
            {
                bool val;
                bool.TryParse(autoDetect, out val);
                AutoDetect = val;
            }
            if (debug != null)
            {
                bool val;
                bool.TryParse(debug, out val);
                Debug = val;
            }
            if (autoDetectString != null)
            {
                AutoDetectString = autoDetectString;
            }
            if (autoDetectExpectedAnswer != null)
            {
                AutoDetectExpectedAnswer = autoDetectExpectedAnswer;
            }
            if (sPort != null)
            {
                SerialPort = sPort;
            }
            if (baud != null)
            {
                int.TryParse(baud, out int val);
                BaudRate = val;
            }
            if (timeout != null)
            {
                int.TryParse(timeout, out int val);
                Timeout = val;
            }
        }
	}
}

