using System;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
	public class DMMResistanceMeasurementResponseModel
	{
        public MeasurementIdentifier Identifier { get; set; }
        public bool ErrorHappened { get; set; }
        public string ErrorMessage { get; set; }
        public double Resistance { get; set; }
}
}

