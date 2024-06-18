using System;
using System.IO.Ports;

namespace SiPMTesterInterface.Models
{
	public class DAQDeviceSettings
    {
		public bool Enabled {get; set; } = true;
		public bool SaveRootFile { get; set; } = true;
		public bool SendWaveformData {get; set; } = false;

        public DAQDeviceSettings(IConfiguration config)
        {
            var enabled = config["DAQDevice:Enabled"];
            var saveRootFile = config["DAQDevice:AutoDetect"];
            var sendWaveformData = config["DAQDevice:AutoDetectString"];

            if (enabled != null)
            {
                bool val;
                bool.TryParse(enabled, out val);
                Enabled = val;
            }
            if (saveRootFile != null)
            {
                bool val;
                bool.TryParse(saveRootFile, out val);
                SaveRootFile = val;
            }
            if (sendWaveformData != null)
            {
                bool val;
                bool.TryParse(sendWaveformData, out val);
                SendWaveformData = val;
            }
        }
    }
}

