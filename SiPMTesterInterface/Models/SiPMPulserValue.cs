using System;
namespace SiPMTesterInterface.Models
{
	public class SiPMPulserValue
	{
		public CurrentSiPMModel SiPM { get; set; } = new CurrentSiPMModel();
		public int PulserValue { get; set; } = 100;

		public SiPMPulserValue()
		{
		}

        public SiPMPulserValue(CurrentSiPMModel sipm, int pulserValue)
        {
			SiPM = sipm;
			PulserValue = pulserValue;
        }
    }
}

