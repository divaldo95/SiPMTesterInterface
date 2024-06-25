using System;
namespace SiPMTesterInterface.Models
{
	public class ModuleTemperatures
	{
        public int Block { get; set; }
		public int Module { get; set; }
        public double[] Temperatures { get; set; } = new double[8];
        public long Timestamp { get; set; }

        public ModuleTemperatures()
		{
		}
	}
}

