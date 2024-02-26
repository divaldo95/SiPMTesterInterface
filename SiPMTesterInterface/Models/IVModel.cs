using System;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Models
{
	public class IVModel
	{
        public ConnectionState ConnectionState { get; set; } = ConnectionState.Disconnected;
        public MeasurementState MeasurementState { get; set; } = MeasurementState.Unknown; //individual measurement state
        public CurrentSiPMModel CurrentSiPM { get; set; } = new CurrentSiPMModel();
    }
}

