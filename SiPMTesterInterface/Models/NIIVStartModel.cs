﻿using System;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
	public class NIIVStartModel
    {
        public MeasurementIdentifier Identifier { get; set; } = new MeasurementIdentifier();
        public double OperatingVoltage { get; set; } = 0;
        public List<double> Voltages { get; set; } = new List<double>();
        public IVSettings IVSettings { get; set; } = new IVSettings();
    }
}

