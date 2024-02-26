using System;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Models
{
	public class SPSModel
	{
        public ConnectionState ConnectionState { get; set; } = ConnectionState.Disconnected;
        public MeasurementState MeasurementState { get; set; } = MeasurementState.Unknown; //individual measurement state
        public List<CurrentSiPMModel> CurrentSiPMs { get; set; } = new List<CurrentSiPMModel>();
        public double CurrentVoltage { get; set; }
    }
}

