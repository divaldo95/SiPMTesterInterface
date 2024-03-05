using System;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json.Linq;
using SiPMTesterInterface.ClientApp.Services;
using SiPMTesterInterface.Controllers;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;
using SiPMTesterInterface.Interfaces;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
    public class ConnectionStateChangedEventArgs : EventArgs
    {
        public ConnectionStateChangedEventArgs() : base()
        {
        }

        public ConnectionStateChangedEventArgs(ConnectionState s, ConnectionState p) : base()
        {
            Previous = p;
            State = s;
        }
        public ConnectionState Previous { get; set; } = ConnectionState.NotConnected;
        public ConnectionState State { get; set; } = ConnectionState.NotConnected;
    }

    public class MeasurementStateChangedEventArgs : EventArgs
    {
        public MeasurementStateChangedEventArgs() : base()
        {
        }

        public MeasurementStateChangedEventArgs(MeasurementState s, MeasurementState p) : base()
        {
            Previous = p;
            State = s;
        }
        public MeasurementState Previous { get; set; } = MeasurementState.Unknown;
        public MeasurementState State { get; set; } = MeasurementState.Unknown;
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

    /*
     * SiPM instruments class implements a RequestSocket and a Subscriber socket.
     * RequestSocket is used for constantly querying the state of the instrument,
     * while SubscriberSocket is used for receiving instant updates.
     * If the SubscriberSocket fails due to network errors, etc, the 
     * RequestSocket keeps trying to get the current states.
     * Both sockets are reconnecting automatically.
     */
    public abstract class SiPMInstrument : ISiPMInstrument
    {
        protected ReqSocket reqSocket;
        protected SubSocket subSocket;

        protected MessageBuffer logBuffer;

        private readonly ILogger<SiPMInstrument> _logger;
        public string InstrumentName { get; private set; }
        private readonly object _updateLock = new object();

        private ConnectionState _ConnectionState { get; set; } = ConnectionState.Disconnected;
        private MeasurementState _MeasurementState { get; set; } = MeasurementState.Unknown;

        /* Save those commands which needs a response data from the instrument
         * Most likely while the pub-sub socket works this cleared automatically.
         * In any case it is not, try to query it once again after a timeout
         */
        protected List<WaitingResponseData> waitingResponseDatas = new List<WaitingResponseData>();
        protected int MeasurementID { get; set; } = 0; //give a unique ID for every measurement

        public void RemoveFromWaitingResponseData(MeasurementIdentifier id)
        {
            waitingResponseDatas.RemoveAll(item => item.Identifier == id); //Got the results
        }

        public ConnectionState ConnectionState
        {
            get
            {
                return _ConnectionState;
            }
            private set
            {
                if (value != _ConnectionState)
                {
                    ConnectionStateChangedEventArgs eventArgs = new ConnectionStateChangedEventArgs();
                    eventArgs.Previous = _ConnectionState;
                    _ConnectionState = value;
                    eventArgs.State = _ConnectionState;
                    OnConnectionStateChanged?.Invoke(this, eventArgs);
                }
            }
        }

        public MeasurementState MeasurementState
        {
            get
            {
                return _MeasurementState;
            }
            private set
            {
                if (value != _MeasurementState)
                {
                    MeasurementStateChangedEventArgs eventArgs = new MeasurementStateChangedEventArgs();
                    eventArgs.Previous = _MeasurementState;
                    _MeasurementState = value;
                    eventArgs.State = _MeasurementState;
                    OnMeasurementStateChanged?.Invoke(this, eventArgs);
                }
            }
        }

        public void AddWaitingResponseData(MeasurementIdentifier identifier)
        {
            WaitingResponseData d = new WaitingResponseData(identifier, RequeryMeasurementResults, HandleGettingMeasurementResultsFail);
            d.Start(TimeSpan.FromSeconds(30));
            waitingResponseDatas.Add(d);
        }

        public void RequeryMeasurementResults(WaitingResponseData responseData)
        {
            string msg = "SendResultsAgain:" + JsonSerializer.Serialize(responseData.Identifier);
            RunCommand(msg);
        }

        public void HandleGettingMeasurementResultsFail(WaitingResponseData responseData)
        {
            string msg = "Failed to get results for " + responseData.Identifier.ToString();
            _logger.LogError(msg);
            RemoveFromWaitingResponseData(responseData.Identifier);
        }

        public event EventHandler<ConnectionStateChangedEventArgs> OnConnectionStateChanged;
        public event EventHandler<MeasurementStateChangedEventArgs> OnMeasurementStateChanged;

        public event EventHandler<IVMeasurementDataReceivedEventArgs> OnIVMeasurementDataReceived;
        public event EventHandler<DMMMeasurementDataReceivedEventArgs> OnDMMMeasurementDataReceived;

        private readonly string reqSocIP;
        private readonly string subSocIP;

        protected bool Enabled { get; set; } = false;

        /*
         * Replacing the test code with JSON structures
         */
        private void OnMessageReceivedCallback(object? sender, MessageReceivedEventArgs resp)
        {
            ProcessResponseString(resp.Message);
        }

        private void OnMessageReceiveFailCallback(object? sender, MessageReceiveFailEventArgs resp)
        {
            ProcessResponseString(resp.Message, false);
        }

        private void OnLogMessageReceived(object? sender, LogMessageReceivedEventArgs e)
        {
            _logger.LogInformation($"{e.Message}");
        }

        private void OnMeasurementMessageReceived(object? sender, MeasurementMessageReceivedEventArgs e)
        {
            ProcessResponseString(e.Message);
        }

        private void OnLogBufferInconsistency(object? sender, MessageBufferIncosistencyEventArgs e)
        {
            _logger.LogInformation("Log buffer is now inconsistent");
        }

        /*
         * reqSocket used for query states of the measurement software.
         * Query messages can be added by reqSocket.AddQueryMessage("state")
         * If a message arrives from the server the OnMessageReceivedCallback function is called
         * If the server went offline or anything happened with the connection (error happened) the OnMessageReceiveFailCallback function is called
         */
        public SiPMInstrument(string InstrumentName, string ip, int controlPort, int logPort, TimeSpan period, ILogger<SiPMInstrument> logger)
        {
            _logger = logger;

            this.InstrumentName = InstrumentName;

            reqSocIP = "tcp://" + ip + ":" + controlPort.ToString();
            subSocIP = "tcp://" + ip + ":" + logPort.ToString();
            Console.WriteLine($"Listening REQ on {reqSocIP}");
            Console.WriteLine($"Listening SUB on {subSocIP}");

            logBuffer = new MessageBuffer();
            logBuffer.OnMessageBufferIncosistency += OnLogBufferInconsistency;

            reqSocket = new ReqSocket(reqSocIP, period);
            reqSocket.OnMessageReceived += OnMessageReceivedCallback;
            reqSocket.OnMessageReceiveFail += OnMessageReceiveFailCallback;

            subSocket = new SubSocket(subSocIP);
            subSocket.OnLogMessageReceived += OnLogMessageReceived;
            subSocket.OnMeasurementMessageReceived += OnMeasurementMessageReceived;

            //automatically query these strings
            reqSocket.AddQueryMessage("Ping:{\"Status:\": 1}"); //Add some placeholder json
            reqSocket.AddQueryMessage("GetState:{\"Status:\": 1}");
        }

        public void Start()
        {
            reqSocket.Start();
            subSocket.Start();
            //StartSub();
        }

        public void RunCommand(string command)
        {
            reqSocket.RunCommand(command);
        }

        public void ProcessResponseString(string response, bool isReceiveOK = true)
        {
            _logger.LogInformation($"Processing {response}...");
            IVMeasurementResponseModel? ivRespModel = null;
            DMMResistanceMeasurementResponseModel? dmmRespModel = null;
            StatusChangeResponseModel? statusRespModel = null;
            MeasurementStartResponseModel? startResponseModel = null;
            MeasurementIdentifier? measurementIdentifier = null;

            string sender;
            string error;
            JObject obj;

            bool parseSuccessful = Parser.ParseMeasurementStatus(response, out sender, out obj);

            if (sender == "Pong" || sender == "Ping" || sender == "Ping:Pong")
            {
                ConnectionState = isReceiveOK ? ConnectionState.Connected : ConnectionState.Disconnected;
            }

            if (!isReceiveOK)
            {
                return;
            }

            if (!parseSuccessful)
            {
                _logger.LogInformation($"Failed to parse {response}");
                return;
            }

            if (obj.Property("State") != null && Parser.JObject2JSON(obj, out statusRespModel, out error))
            {
                MeasurementState = statusRespModel.State;
                return;
            }
            else if (obj.Property("Identifier") == null)
            {
                return; //Not the droids we are looking for
            }
            else if (!Parser.JObject2JSON(obj, out measurementIdentifier, out error))
            {
                return;
            }

            else if (obj.Property("Successful") != null && Parser.JObject2JSON(obj, out startResponseModel, out error))
            {
                if (startResponseModel.Successful)
                {
                    AddWaitingResponseData(startResponseModel.Identifier);
                }
                else
                {
                    //handle error
                    _logger.LogError(startResponseModel.ErrorMessage);
                }
            }

            switch (measurementIdentifier.Type)
            {
                case MeasurementType.IVMeasurement:
                    if (Parser.JObject2JSON(obj, out ivRespModel, out error))
                    {
                        RemoveFromWaitingResponseData(ivRespModel.Identifier);
                        OnIVMeasurementDataReceived?.Invoke(this, new IVMeasurementDataReceivedEventArgs(ivRespModel));
                        //save data
                    }
                    break;
                case MeasurementType.SPSMeasurement:

                    break;
                case MeasurementType.DMMResistanceMeasurement:
                    if (Parser.JObject2JSON(obj, out dmmRespModel, out error))
                    {
                        RemoveFromWaitingResponseData(dmmRespModel.Identifier);
                        OnDMMMeasurementDataReceived?.Invoke(this, new DMMMeasurementDataReceivedEventArgs(dmmRespModel));
                        //save data
                    }
                    break;
                default:
                    _logger.LogError($"Unknown type of response message received: {measurementIdentifier.Type}");
                    break;
            }
        }
    }
}

