using System;
using SiPMTesterInterface.Models;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Interfaces
{
	public interface IStateContext
	{
		public Task ReceiveGlobalStateChange(GlobalStateModel g);
		public Task ReceiveIVMeasurementStateChange(MeasurementState i);
        public Task ReceiveSPSMeasurementStateChange(MeasurementState s);
    }
}

