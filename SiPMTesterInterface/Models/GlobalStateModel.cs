using System;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Models
{
	public class GlobalStateModel
	{
		public MeasurementStartModel CurrentRun { get; set; } = new MeasurementStartModel();
		public AllStatusModel AllStatus { get; set; } = new AllStatusModel();
		public MeasurementState GlobalIVMeasurementState = MeasurementState.Unknown; //Ongoing measurement
        public MeasurementState GlobalSPSMeasurementState = MeasurementState.Unknown; //Ongoing measurement
		public IVSettings GlobalIVSettings = new IVSettings();
        public IVModel IVModel { get; set; } = new IVModel();
		public SPSModel SPSModel { get; set; } = new SPSModel();
		//Placeholder for SPSSettings
	}
}

