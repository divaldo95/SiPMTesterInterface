using System;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
	public class CoolerSettingsModel
	{
		public int Block { get; set; } = 0;
		public int Module { get; set; } = 0;
		public bool Enabled { get; set; } = false;
		public bool EnabledByUser { get; set; } = false;
		public double TargetTemperature { get; set; } = 25.0;
		public int FanSpeed { get; set; } = 10;
		public ModuleCoolerState State { get; set; } = new ModuleCoolerState();
        public double[] Temperatures { get; set; } = new double[8];
        public CoolerSettingsModel()
		{

		}
	}
}

