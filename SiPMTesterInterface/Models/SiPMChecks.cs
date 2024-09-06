using System;
namespace SiPMTesterInterface.Models
{
	public class SiPMChecks
	{
        public bool SelectedForMeasurement { get; set; } = false;
        public bool DMMResistanceOK { get; set; } = false;

        public bool RForwardDone { get; set; } = false;
        public bool RForwardOK { get; set; } = false;

        public bool IDarkDone { get; set; } = false;
        public bool IDarkOK { get; set; } = false;

        public bool IVDone { get; set; } = false;
        public bool IVTemperatureOK { get; set; } = false;
        public bool IVMeasurementOK { get; set; } = false;
        public bool IVVoltageCheckOK { get; set; } = false;
        public bool IVCurrentCheckOK { get; set; } = false;
        public bool IVVbrOK { get; set; } = false;

        public SiPMChecks()
		{
		}
	}
}

