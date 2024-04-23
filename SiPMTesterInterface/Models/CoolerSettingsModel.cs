using System;
namespace SiPMTesterInterface.Models
{
	public class CoolerSettingsModel
	{
		public int Block { get; set; }
		public int Module { get; set; }
		public bool Enabled { get; set; }
		public double TargetTemperature { get; set; }
		public int FanSpeed { get; set; }
        public CoolerSettingsModel()
		{
		}
	}
}

