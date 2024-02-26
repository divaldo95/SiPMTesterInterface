using System;
using System.Reflection;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
    public class NIMachineSettings
    {
        public bool Enabled { get; set; } = false;
        public string IP { get; set; } = "127.0.0.1";
        public int CommandPort { get; set; } = 0;
        public int LogPort { get; set; } = 0;
        public int UpdatePeriodms { get; set; } = 0;

        public NIMachineSettings(IConfiguration config)
        {
            var niMachineEnabled = config["NIMachine:Enabled"];
            var ip = config["NIMachine:IP"];
            var commandPort = config["NIMachine:CommandPort"];
            var logPort = config["NIMachine:LogPort"];
            var updatePeriodms = config["NIMachine:updatePeriodms"];

            if (niMachineEnabled != null)
            {
                bool val;
                bool.TryParse(niMachineEnabled, out val);
                Enabled = val;
            }
            if (ip != null)
            {
                IP = ip;
            }
            if (commandPort != null)
            {
                int val;
                int.TryParse(commandPort, out val);
                CommandPort = val;
            }
            if (logPort != null)
            {
                int val;
                int.TryParse(logPort, out val);
                LogPort = val;
            }
            if (updatePeriodms != null)
            {
                int val;
                int.TryParse(updatePeriodms, out val);
                UpdatePeriodms = val;
            }
        }
    }

	public class NIMachine : SiPMInstrument
	{
        private object _lockObject = new object();

        public void AddWaitingResponseData(NIMachineStartModel measurementData)
        {
            WaitingResponseData d = new WaitingResponseData(measurementData.Identifier, RequeryMeasurementResults, HandleGettingMeasurementResultsFail);
            d.Start(TimeSpan.FromSeconds(30));
            waitingResponseDatas.Add(d);
        }

        public void StartIVMeasurement(NIMachineStartModel measurementData)
        {
            string msg = "StartIVMeasurement:" + System.Text.Json.JsonSerializer.Serialize(measurementData);
            base.reqSocket.RunCommand(msg);
        }

        public void StartDMMResistanceMeasurement(DMMResistanceModel measurementData)
        {
            string msg = "StartDMMResistanceMeasurement:" + System.Text.Json.JsonSerializer.Serialize(measurementData);
            base.reqSocket.RunCommand(msg);
        }

        public NIMachine(IConfiguration config, ILogger<NIMachine> logger) : this(new NIMachineSettings(config), logger)
        {

        }

        public NIMachine(NIMachineSettings settings, ILogger<NIMachine> logger) : this(settings.Enabled, settings.IP, settings.CommandPort, settings.LogPort, TimeSpan.FromMilliseconds(settings.UpdatePeriodms), logger)
        {
        }

		public NIMachine(bool enabled, string ip, int controlPort, int logPort, TimeSpan period, ILogger<NIMachine> logger) :
			base("NIMachine", ip, controlPort, logPort, period, logger)
		{
            base.reqSocket.OnMessageReceived += ReqSocket_OnMessageReceived;
            base.reqSocket.OnMessageReceiveFail += ReqSocket_OnMessageReceiveFail;
            //base.reqSocket.AddQueryMessage("GetCurrentSiPM");
            if (enabled)
            {
                base.Start();
            }
		}

        /*
         * Structure of incoming data: Object:Data
         * Object: The object the data is for
         * Data: Status update
         */
        private void ReqSocket_OnMessageReceived(object? sender, MessageReceivedEventArgs resp)
        {
            //Console.WriteLine($"Receive OK event NIMachine: {resp.Message}");
            //ProcessResponseString(resp.Message);
        }

        /*
         * When the service fails to query the client
         * this function is called.
         * It can be used to detect failure of network or service on the other end.
         */
        private void ReqSocket_OnMessageReceiveFail(object? sender, MessageReceiveFailEventArgs resp)
        {
            //Console.WriteLine($"Receive failed event NIMachine: {resp.Message}");
            //ProcessResponseString(resp.Message, false);
        }
	}
}
