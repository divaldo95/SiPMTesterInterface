using System;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Models
{
	public class SPSModel
	{
        public ConnectionState ConnectionState { get; set; } = ConnectionState.Disconnected;
        public MeasurementState MeasurementState { get; set; } = MeasurementState.Unknown;
        public List<CurrentSiPMModel> CurrentSiPM { get; set; } = new List<CurrentSiPMModel>();
        public double CurrentVoltage { get; set; }
    }
}

