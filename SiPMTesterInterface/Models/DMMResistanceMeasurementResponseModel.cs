using System;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
	public class DMMResistanceMeasurementResponseModel
	{
        public MeasurementIdentifier Identifier { get; set; }
        public long StartTimestamp { get; set; }
        public long EndTimestamp { get; set; }
        public bool ErrorHappened { get; set; }
        public string ErrorMessage { get; set; }
        public int CorrectionPercentage { get; set; }
        public List<double> Voltages { get; set; }
        public List<double> Currents { get; set; }
        public double Resistance { get; set; }
}
}

