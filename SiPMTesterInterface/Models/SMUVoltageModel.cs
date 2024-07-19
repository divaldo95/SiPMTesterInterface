using System;
namespace SiPMTesterInterface.Models
{
	public class SMUVoltageModel
	{
        public double Voltage { get; set; }
        public double CurrentLimit { get; set; }
        public double CurrentLimitRange { get; set; }
        public int Iterations { get; set; }
        public double VoltageRange { get; set; }
    }
}

