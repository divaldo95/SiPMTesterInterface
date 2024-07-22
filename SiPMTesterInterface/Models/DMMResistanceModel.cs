using System;
namespace SiPMTesterInterface.Models
{
	public class DMMResistanceModel
	{
		public double Voltage { get; set; } = 30;
		public int Iterations { get; set; } = 5;
		public int CorrectionPercentage { get; set; } = 10;
    }
}

