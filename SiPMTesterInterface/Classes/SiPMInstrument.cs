using System;
using System.Text;
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
        private readonly RequestSocket reqSocket = new RequestSocket();
        private NetMQPoller poller;

        private readonly ILogger<SiPMInstrument> _logger;
        public string InstrumentName { get; private set; }
        private readonly object _lockObject = new object();
        private Timer _timer;
        private readonly object _updateLock = new object();

        private const int MaxMemoryCapacityMB = 10; // Adjust the limit as needed
        private const int MaxMessageSizeMB = 1; // Adjust the limit as needed

        private readonly List<string> messageBuffer = new List<string>();
        private readonly object bufferLock = new object();

        private long currentMemoryUsage = 0;

        public ConnectionState ConnectionState { get; private set; }
        public MeasurementState MeasurementState { get; private set; }
        private readonly string reqSocIP;
        private readonly string subSocIP;
        private readonly TimeSpan Period;

        protected bool Enabled { get; set; } = false;

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
                            HandleMessage(message);
                        }
                    };

                    poller.Run();
                }
            }
        }

        protected abstract void HandleMessage(string message);

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

        static int cnt = 0;
        protected bool AskServer(string message, out string response)
        {
            bool success = false;
            string received = "";
            lock (_lockObject)
            {
                success = (reqSocket.TrySendFrame(TimeSpan.FromMilliseconds(800), message) && reqSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(800), out received));
            }
            cnt++;
            response = received;
            //_logger.LogInformation(response);
            return success;
        }

        protected abstract void PeriodicUpdate();

        private void TimerCallback(object? state)
        {
            // Query ConnectionState and MeasurementState
            ConnectionState connectionState = GetConnectionState();
            MeasurementState measurementState = GetMeasurementState();
            PeriodicUpdate();
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

        public void StopTimer()
        {
            // Stop the timer when needed
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }

        public SiPMInstrument(string InstrumentName, string ip, int controlPort, int logPort, TimeSpan period, ILogger<SiPMInstrument> logger)
        {
            _logger = logger;

            this.InstrumentName = InstrumentName;

            reqSocIP = "tcp://" + ip + ":" + controlPort.ToString();
            subSocIP = "tcp://" + ip + ":" + logPort.ToString();
            Period = period;
            //reqSocket.Connect("tcp://192.168.0.45:5556");
            //reqSocket.Connect(reqSocIP);
            //StartSubscriber(subSocIP);

            //_timer = new Timer(TimerCallback, null, TimeSpan.Zero, period); // Change the interval as needed


        }

        public void Start()
        {
            reqSocket.Connect(reqSocIP);
            //StartSubscriber(subSocIP);
            Task subscriberTask = Task.Run(() => StartSubscriber(subSocIP));

            _timer = new Timer(TimerCallback, null, TimeSpan.Zero, Period); // Change the interval as needed
        }

        public ConnectionState GetConnectionState()
        {
            //reqSocket.SendFrame("Hello");

            string received = "";
            bool success = AskServer("ping", out received);
            if (success)
            {
                if (received.Equals("pong"))
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
            bool success = AskServer("GetState", out received);
            if (success)
            {
                int.TryParse(received, out state);
                return (MeasurementState)state;
            }
            else
            {
                return MeasurementState.Unknown;
            }
        }
    }
}

