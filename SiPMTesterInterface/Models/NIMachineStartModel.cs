using System;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
	public class NIMachineStartModel
    {
        public MeasurementIdentifier Identifier { get; set; } = new MeasurementIdentifier();
        public CurrentSiPMModel CurrentSiPM { get; set; } = new CurrentSiPMModel();
        public List<double> Voltages { get; set; } = new List<double>();
        public IVSettings IVSettings { get; set; } = new IVSettings();
        public bool MeasureDMMResistance { get; set; } = false;
        public DMMResistanceModel DMMResistance { get; set; } = new DMMResistanceModel();
    }
}

