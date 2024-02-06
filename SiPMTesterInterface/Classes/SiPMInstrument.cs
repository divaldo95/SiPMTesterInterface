using System;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using SiPMTesterInterface.ClientApp.Services;
using SiPMTesterInterface.Controllers;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Interfaces;

namespace SiPMTesterInterface.Classes
{
    public abstract class SiPMInstrument : ISiPMInstrument
    {
        protected ReqSocket reqSocket;
        protected SubSocket subSocket;

        protected MessageBuffer logBuffer;

        private readonly ILogger<SiPMInstrument> _logger;
        public string InstrumentName { get; private set; }
        private readonly object _updateLock = new object();

        public ConnectionState ConnectionState { get; private set; }
        public MeasurementState MeasurementState { get; private set; }

        private readonly string reqSocIP;
        private readonly string subSocIP;

        protected bool Enabled { get; set; } = false;

        private void UpdateMeasurementState(string data, bool isReceiveOK)
        {
            int state = (int)MeasurementState.Unknown;
            if (isReceiveOK)
            {
                int.TryParse(data, out state);
            }
            lock (_updateLock)
            {
                MeasurementState = (MeasurementState)state;
            }
        }

        private void UpdateConnectionState(string data, bool isReceiveOK)
        {
            if (!isReceiveOK)
            {
                ConnectionState = ConnectionState.Disconnected;
                return;
            }
            if (data.Equals("pong"))
            {
                ConnectionState = ConnectionState.Connected;
            }
            else
            {
                ConnectionState = ConnectionState.Error;
            }
        }

        private void UpdateState(string obj, string data, bool isReceiveOK)
        {
            switch (obj)
            {
                case "GetState":
                    UpdateMeasurementState(data, isReceiveOK);
                    break;
                case "Ping":
                    UpdateConnectionState(data, isReceiveOK);
                    break;
                default:
                    break;
            }
        }

        private void OnMessageReceivedCallback(object? sender, MessageReceivedEventArgs resp)
        {
            string[] datas = resp.Message.Split(':');
            if (datas.Length != 2)
            {
                return;
            }

            string obj = datas[0];
            string data = datas[1];

            UpdateState(obj, data, true);
        }

        private void OnMessageReceiveFailCallback(object? sender, MessageReceiveFailEventArgs resp)
        {
            string obj = resp.Message;
            string data = "";

            UpdateState(obj, data, false);
        }

        private void OnLogMessageReceived(object? sender, LogMessageReceivedEventArgs e)
        {
            _logger.LogInformation(e.Message);
        }

        private void OnMeasurementMessageReceived(object? sender, MeasurementMessageReceivedEventArgs e)
        {
            _logger.LogInformation(e.Message);
        }

        private void OnMessageReceivedIntervalElapsed(object? sender, MessageReceivedIntervalElapsedEventArgs e)
        {
            /* A timer set to check intervals between messages. If the last message arrived late
             * this timer will fire this event. In that case the other side can be asked whether it
             * is alive or resend the necessary data.
             */
            _logger.LogInformation("Message received time interval elapsed");
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
            reqSocket.AddQueryMessage("Ping");
            reqSocket.AddQueryMessage("GetState");
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
    }
}

