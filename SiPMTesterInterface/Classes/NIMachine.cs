using System;
using System.Reflection;
using Newtonsoft.Json;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;
using SiPMTesterInterface.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    public class IVMeasurementDataReceivedEventArgs : EventArgs
    {
        public IVMeasurementDataReceivedEventArgs() : base()
        {
            Data = new IVMeasurementResponseModel();
        }

        public IVMeasurementDataReceivedEventArgs(IVMeasurementResponseModel d) : base()
        {
            Data = d;
        }
        public IVMeasurementResponseModel Data { get; set; }
    }

    public class DMMMeasurementDataReceivedEventArgs : EventArgs
    {
        public DMMMeasurementDataReceivedEventArgs() : base()
        {
            Data = new DMMResistanceMeasurementResponseModel();
        }

        public DMMMeasurementDataReceivedEventArgs(DMMResistanceMeasurementResponseModel d) : base()
        {
            Data = d;
        }
        public DMMResistanceMeasurementResponseModel Data { get; set; }
    }

    public class MeasurementStartEventArgs : EventArgs
    {
        public MeasurementStartEventArgs() : base()
        {
            Response = new MeasurementStartResponseModel();
        }

        public MeasurementStartEventArgs(MeasurementStartResponseModel r) : base()
        {
            Response = r;
        }
        public MeasurementStartResponseModel Response { get; set; }
    }

    public class NIMachine : SiPMInstrument
	{
        private object _lockObject = new object();

        public event EventHandler<IVMeasurementDataReceivedEventArgs> OnIVMeasurementDataReceived;
        public event EventHandler<DMMMeasurementDataReceivedEventArgs> OnDMMMeasurementDataReceived;
        public event EventHandler<MeasurementStartEventArgs> OnMeasurementStartSuccess;
        public event EventHandler<MeasurementStartEventArgs> OnMeasurementStartFail;

        public void StartIVMeasurement(NIIVStartModel measurementData)
        {
            string msg = "StartIVMeasurement:" + JsonConvert.SerializeObject(measurementData);
            base.reqSocket.RunCommand(msg);
        }

        public void StartDMMResistanceMeasurement(NIDMMStartModel measurementData)
        {
            string msg = "StartDMMMeasurement:" + JsonConvert.SerializeObject(measurementData);
            Console.WriteLine($"Sent command: {msg}");
            base.reqSocket.RunCommand(msg);
        }

        public void StopMeasurement()
        {
            if (!Initialized)
            {
                return;
            }
            string msg = "StopMeasurement:{\"Status:\": 1}";
            if (this.ConnectionState != ConnectionState.Connected)
            {
                reqSocket.AddQueryMessage(msg);
                _logger.LogError("NI Machine is unavailable, stop command queued");
            }
            else
            {
                base.reqSocket.RunCommand(msg);
                Console.WriteLine($"Sent command: {msg}");
            }
        }

        public new void Stop()
        {
            //stop everything here
            base.Stop();
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
            OnJSONResponseReceived += OnGenericResponseReceived;
            Enabled = enabled;
		}

        public new void Init()
        {
            base.Init();
        }

        /*
         * NI machine related possible cases:
         * - Measurement started successfully
         * - Measurement start failed
         * - IV measurement done, data received
         * - DMM resistance measurement done, data received
         * Handle all of them here.
         */
        private void OnGenericResponseReceived(object? sender, JSONResponseReceivedEventArgs e)
        {
            string error = "";

            //costs less to check wheter it has an Identifier property than try parsing it
            if (e.Response.jsonObject.Property("Identifier") == null)
            {
                return; //Not the droids we are looking for
            }

            if (e.Response.Sender == "IVMeasurementDone" || e.Response.Sender == "IVMeasurementResult")
            {
                IVMeasurementResponseModel respModel;
                if (Parser.JObject2JSON(e.Response.jsonObject, out respModel, out error))
                {
                    if (respModel.ErrorHappened)
                    {
                        _logger.LogError(respModel.ErrorMessage);
                    }
                    RemoveFromWaitingResponseData(respModel.Identifier);
                    OnIVMeasurementDataReceived?.Invoke(this, new IVMeasurementDataReceivedEventArgs(respModel));
                }
                return;
            }

            else if (e.Response.Sender == "DMMMeasurementDone" || e.Response.Sender == "DMMMeasurementResult")
            {
                DMMResistanceMeasurementResponseModel respModel;
                if (Parser.JObject2JSON(e.Response.jsonObject, out respModel, out error))
                {
                    if (respModel.ErrorHappened)
                    {
                        _logger.LogError(respModel.ErrorMessage);
                    }
                    RemoveFromWaitingResponseData(respModel.Identifier);
                    OnDMMMeasurementDataReceived?.Invoke(this, new DMMMeasurementDataReceivedEventArgs(respModel));
                }
                return;
            }

            else if (e.Response.Sender == "MeasurementStart")
            {
                MeasurementStartResponseModel respModel;
                if (!Parser.JObject2JSON(e.Response.jsonObject, out respModel, out error))
                {
                    _logger.LogError($"Failed to parse MeasurementStart event ({error})");
                    return;
                }
                if (respModel.Successful)
                {
                    AddWaitingResponseData(respModel.Identifier);
                    OnMeasurementStartSuccess?.Invoke(this, new MeasurementStartEventArgs(respModel));
                }
                else
                {
                    OnMeasurementStartFail?.Invoke(this, new MeasurementStartEventArgs(respModel));
                }
                return;
            }

            else if (e.Response.Sender == "StopMeasurement")
            {
                _logger.LogInformation("Stop measurement command ack from NI Machine");
                return;
            }
        }
	}
}
