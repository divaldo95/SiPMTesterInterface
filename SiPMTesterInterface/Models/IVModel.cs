using System;
namespace SiPMTesterInterface.Models
{
	public class IVModel
	{
        public string State { get; set; }
        public int CurrentArray { get; set; }
        public int CurrentSiPM { get; set; }
        public double Voltage { get; set; }
    }
}

