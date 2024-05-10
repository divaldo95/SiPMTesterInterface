using System;
namespace SiPMTesterInterface.Models
{
	public class IVAnalysationResult
	{
		public bool Analysed { get; set; } = false;
		public bool IsOK { get; set; } = false;
		public double BreakdownVoltage { get; set; } = 0.0;
		public double CompensatedBreakdownVoltage { get; set; } = 0.0;
		public double ChiSquare { get; set; } = 0.0;
		public string RootFileLocation { get; set; } = "";
        public IVAnalysationResult()
		{
			
		}
	}
}

