﻿using System;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
	public class CurrentMeasurementDataModel
	{
		public bool IsIVDone { get; set; } = false;
		public bool IsSPSDone { get; set; } = false;
		public CurrentSiPMModel SiPMLocation { get; set; }
		public SiPM SiPMMeasurementDetails { get; set; }
		public MeasurementIdentifier IVMeasurementID { get; set; } = new MeasurementIdentifier();
		public MeasurementIdentifier SPSMeasurementID { get; set; } = new MeasurementIdentifier();
        public IVMeasurementResponseModel IVResult { get; set; }
        //placeholder for SPS response
	}
}

