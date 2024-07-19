using System;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Models
{
	public class NIVoltageAndCurrentStartModel
	{
        public MeasurementIdentifier Identifier { get; set; } = new MeasurementIdentifier();
        public VoltageAndCurrentMeasurementTypes MeasurementType { get; set; } = VoltageAndCurrentMeasurementTypes.Unknown;
        public SMUVoltageModel FirstIteration { get; set; } = new SMUVoltageModel();
        public SMUVoltageModel SecondIteration { get; set; } = new SMUVoltageModel();
    }
}

