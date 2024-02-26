using System;
namespace SiPMTesterInterface.Models
{
	public class SerialSettings
	{
        public bool Enabled { get; set; } = true;
        public string SerialPort { get; set; } = "";
        public int BaudRate { get; set; } = 115200;
        public int Timeout { get; set; } = 10000;

		public SerialSettings(IConfiguration config, string obj)
		{
            var enabled = config[obj + ":Enabled"];
            var sPort = config[obj+":SerialPort"];
            var baud = config[obj + ":BaudRate"];
            var timeout = config[obj + ":Timeout"];

            if (enabled != null)
            {
                bool val;
                bool.TryParse(enabled, out val);
                Enabled = val;
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

