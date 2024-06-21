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
        public Task ReceiveGlobalIVMeasurementStateChange(MeasurementState g);
        public Task ReceiveSPSMeasurementStateChange(MeasurementState s);
        public Task ReceiveIVConnectionStateChange(ConnectionState cs);
        public Task ReceiveSiPMIVMeasurementDataUpdate(CurrentSiPMModel c);

        public Task ReceivePulserStateChange(SerialConnectionStateChangedEventArgs s);
        public Task ReceivePulserTempCoolerData(PSoCCommuicatorDataReadEventArgs d);

        public Task ReceiveIVAnalysationResult(CurrentSiPMModel cs, IVMeasurementHubUpdate res);

        public Task ReceiveLogMessage(LogMessageModel logMessage);

        public Task ReceiveCurrentTask(TaskTypes currentTask);

    }
}

