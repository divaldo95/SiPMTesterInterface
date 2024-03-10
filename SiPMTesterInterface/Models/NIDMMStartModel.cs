using System;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
	public class NIDMMStartModel
	{
        public MeasurementIdentifier Identifier { get; set; } = new MeasurementIdentifier();
        public DMMResistanceModel DMMResistance { get; set; } = new DMMResistanceModel();
	}
}

