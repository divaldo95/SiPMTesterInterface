using System;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Models
{
	public class DeviceStatesModel
	{
		public DeviceStates PulserState { get; set; } = DeviceStates.Unknown;
        public DeviceStates HVPSUState { get; set; } = DeviceStates.Unknown;
        public DeviceStates NIMachineState { get; set; } = DeviceStates.Unknown;
        public DeviceStates DAQDeviceState { get; set; } = DeviceStates.Unknown;

        public DeviceStatesModel()
		{
		}
	}
}

