using System;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private NetMQPoller poller;

        private readonly ILogger<SiPMInstrument> _logger;
        public string InstrumentName { get; private set; }
        private readonly object _updateLock = new object();

        private const int MaxMemoryCapacityMB = 10; // Adjust the limit as needed
        private const int MaxMessageSizeMB = 1; // Adjust the limit as needed

        private readonly List<string> messageBuffer = new List<string>();
        private readonly object bufferLock = new object();

        private long currentMemoryUsage = 0;

        public ConnectionState ConnectionState { get; private set; }
        public MeasurementState MeasurementState { get; private set; }

        //public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        //public event EventHandler<MessageReceiveFailEventArgs> OnMessageReceiveFail;

        private readonly string reqSocIP;
        private readonly string subSocIP;
        /*
        private readonly TimeSpan Period;

        private const int RequestTimeout = 2500;
        private const int RequestRetries = 3;

        private int sequence = 0;
        private bool expectReply = true;
        private int retriesLeft = RequestRetries;
        private string outString = "";
        */

        protected bool Enabled { get; set; } = false;

        /*
        void TerminateClient(NetMQSocket client)
        {
            client.Disconnect(reqSocIP);
            client.Close();
        }

        RequestSocket CreateServerSocket()
        {
            Console.WriteLine("C: Connecting to server...");

            var client = new RequestSocket();
            client.Connect(reqSocIP);
            client.Options.Linger = TimeSpan.Zero;
            client.ReceiveReady += ClientOnReceiveReady;

            return client;
        }
        */

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
            //OnMessageReceived?.Invoke(sender, resp);
        }

        private void OnMessageReceiveFailCallback(object? sender, MessageReceiveFailEventArgs resp)
        {
            string obj = resp.Message;
            string data = "";

            UpdateState(obj, data, false);
            //OnMessageReceiveFail?.Invoke(sender, resp);
        }

        /*
        void ClientOnReceiveReady(object? sender, NetMQSocketEventArgs args)
        {
            outString = args.Socket.ReceiveFrameString();
            //string strReply = Encoding.Unicode.GetString(reply);

            if (true)
            {
                Console.WriteLine("C: Server replied OK ({0})", outString);
                retriesLeft = RequestRetries;
                expectReply = false;
            }
            else
            {
                Console.WriteLine("C: Malformed reply from server: {0}", outString);
            }
        }
        */

        public void Stop()
        {
            StopSubscriber();

        }

        public void StopSubscriber()
        {
            if (poller != null)
            {
                poller.StopAsync();
            }
        }

        public void StartSubscriber(string publisherAddress)
        {
            using (var subscriberSocket = new SubscriberSocket())
            {
                subscriberSocket.Connect(publisherAddress);
                subscriberSocket.Subscribe("[LOG]");
                subscriberSocket.Subscribe("[MEAS]");
                using (var poller = new NetMQPoller { subscriberSocket })
                {
                    subscriberSocket.ReceiveReady += (sender, args) =>
                    {
                        lock (bufferLock)
                        {
                            // Receive the message
                            var message = subscriberSocket.ReceiveFrameString();

                            // Process the message (e.g., store in memory)
                            ProcessMessage(message);
                        }
                    };

                    poller.Run();
                }
            }
        }

        private void ProcessMessage(string message)
        {
            // Calculate message size in bytes
            int messageSizeBytes = Encoding.UTF8.GetByteCount(message);

            // Check if adding the message exceeds the memory capacity
            if (currentMemoryUsage + messageSizeBytes > MaxMemoryCapacityMB * 1024 * 1024)
            {
                // If exceeds capacity, remove the oldest message (LIFO)
                if (messageBuffer.Count > 0)
                {
                    string removedMessage = messageBuffer[0];
                    int removedMessageSizeBytes = Encoding.UTF8.GetByteCount(removedMessage);

                    messageBuffer.RemoveAt(0);
                    currentMemoryUsage -= removedMessageSizeBytes;

                    Console.WriteLine($"Message removed from buffer: {removedMessage}");
                }
            }

            // Add the new message to the buffer
            messageBuffer.Add(message);
            currentMemoryUsage += messageSizeBytes;

            Console.WriteLine($"Message received and added to buffer: {message}");
        }

        /*
        protected bool AskServer(string message, out string response)
        {
            bool success = false;
            string received = "";
            lock (_lockObject)
            {
                //success = (reqSocket.TrySendFrame(TimeSpan.FromMilliseconds(5000), message) && reqSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(800), out received));
                while (retriesLeft > 0)
                {
                    sequence++;
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    Console.WriteLine("C: Sending ({0})", sequence);
                    reqSocket.SendFrame(message);
                    expectReply = true;

                    while (expectReply)
                    {
                        success = reqSocket.Poll(TimeSpan.FromMilliseconds(RequestTimeout));

                        if (success)
                        {
                            _timer.Change(Period, Period);
                            break;
                        }
                        retriesLeft--;

                        if (retriesLeft == 0)
                        {
                            reqSocket = CreateServerSocket();
                            _timer.Change(Period, Period);
                            Console.WriteLine("C: Server seems to be offline, abandoning");
                            break;
                        }

                        Console.WriteLine("C: No response from server, retrying...");

                        TerminateClient(reqSocket);

                        reqSocket = CreateServerSocket();
                        reqSocket.SendFrame(message);
                    }
                    if (success)
                        break;
                }
                retriesLeft = RequestRetries;
            }
            response = received;
            
            //_logger.LogInformation(response);
            return success;
        }
        */

        /*
        private void TimerCallback(object? state)
        {
            _logger.LogInformation($"[{InstrumentName}] Timer fired");
            // Query ConnectionState and MeasurementState
            ConnectionState connectionState = GetConnectionState();
            MeasurementState measurementState = GetMeasurementState();
            lock (_updateLock)
            {
                if (ConnectionState != connectionState || MeasurementState != measurementState)
                {
                    _logger.LogInformation($"[{InstrumentName}] Connection State: {connectionState}, Measurement State: {measurementState}");
                }
                ConnectionState = connectionState;
                MeasurementState = measurementState;
            }
            
        }
        */

        public SiPMInstrument(string InstrumentName, string ip, int controlPort, int logPort, TimeSpan period, ILogger<SiPMInstrument> logger)
        {
            _logger = logger;

            this.InstrumentName = InstrumentName;

            reqSocIP = "tcp://" + ip + ":" + controlPort.ToString();
            subSocIP = "tcp://" + ip + ":" + logPort.ToString();
            Console.WriteLine($"Listening REQ on {reqSocIP}");
            Console.WriteLine($"Listening SUB on {subSocIP}");
            reqSocket = new ReqSocket(reqSocIP, period);
            reqSocket.OnMessageReceived += OnMessageReceivedCallback;
            reqSocket.OnMessageReceiveFail += OnMessageReceiveFailCallback;
            //automatically query these strings
            reqSocket.AddQueryMessage("Ping");
            reqSocket.AddQueryMessage("GetState");
        }


        public void StartSub()
        {
            //StartSubscriber(subSocIP);
            Task subscriberTask = Task.Run(() => StartSubscriber(subSocIP));
        }

        public void Start()
        {
            reqSocket.Start();
            //StartSub();
        }

        /*
        public ConnectionState GetConnectionState()
        {
            //reqSocket.SendFrame("Hello");

            string received = "";
            bool success = AskServer("ping");
            if (success)
            {
                if (outString.Equals("pong"))
                {
                    return ConnectionState.Connected;
                }
                else
                {
                    return ConnectionState.Error;
                }
                //_logger.LogInformation(received);

            }
            else
            {
                return ConnectionState.Disconnected;
            }

        }

        public MeasurementState GetMeasurementState()
        {
            //reqSocket.SendFrame("Hello");

            string received = "";
            int state = (int)MeasurementState.Unknown;
            bool success = AskServer("GetState");
            if (success)
            {
                int.TryParse(outString, out state);
                return (MeasurementState)state;
            }
            else
            {
                return MeasurementState.Unknown;
            }
        }
        */

        public void RunCommand(string command)
        {
            reqSocket.RunCommand(command);
        }
    }
}

