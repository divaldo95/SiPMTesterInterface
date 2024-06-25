using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Models;
using SiPMTesterInterface.Interfaces;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Hubs
{
	public class UpdatesHub : Hub<IStateContext>
    {
        public async Task SendGlobalStateModelChange(GlobalStateModel g)
        => await Clients.All.ReceiveGlobalStateChange(g);

        public async Task SendIVMeasurementStateChange(IVModel i)
        => await Clients.All.ReceiveIVMeasurementStateChange(i.MeasurementState);

        public async Task SendGlobalIVMeasurementStateChange(MeasurementState s)
        => await Clients.All.ReceiveGlobalIVMeasurementStateChange(s);

        public async Task SendSPSMeasurementStateChange(SPSModel s)
        => await Clients.All.ReceiveSPSMeasurementStateChange(s.MeasurementState);

        public async Task SendIVConnectionStateChange(ConnectionState cs)
            => await Clients.All.ReceiveIVConnectionStateChange(cs);

        public async Task SendIVAnalysationResult(CurrentSiPMModel cs, IVMeasurementHubUpdate res)
            => await Clients.All.ReceiveIVAnalysationResult(cs, res);

        public async Task SendLogMessage(LogMessageModel errorMessage)
            => await Clients.All.ReceiveLogMessage(errorMessage);

        public async Task SendCurrentTask(TaskTypes currentTask)
            => await Clients.All.ReceiveCurrentTask(currentTask);

        public async Task SendNewModuleCoolerData(ModuleCoolerState coolerData)
            => await Clients.All.ReceiveModuleCoolerData(coolerData);

        public async Task SendNewTemperatureData(TemperaturesArray temp)
            => await Clients.All.ReceiveTemperatureData(temp);

        public async Task SendPulserReadoutIntervalChange(int interval)
            => await Clients.All.ReceivePulserReadoutIntervalChange(interval);

        
    }
}

