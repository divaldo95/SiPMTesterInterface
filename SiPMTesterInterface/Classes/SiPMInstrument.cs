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
        private readonly ILogger<SiPMInstrument> _logger;
        public string InstrumentName { get; private set; }
        private readonly object _lockObject = new object();
        private readonly Timer _timer;
        private readonly object _updateLock = new object();

        private const int MaxMemoryCapacityMB = 10; // Adjust the limit as needed
        private const int MaxMessageSizeMB = 1; // Adjust the limit as needed

        private readonly List<string> messageBuffer = new List<string>();
        private readonly object bufferLock = new object();

        private long currentMemoryUsage = 0;

        public ConnectionState ConnectionState { get; private set; }
        public MeasurementState MeasurementState { get; private set; }

        public void StartSubscriber(string publisherAddress)
        {
            using (var subscriberSocket = new SubscriberSocket())
            {
                subscriberSocket.Connect(publisherAddress);
                subscriberSocket.SubscribeToPrefix("[LOG]");
                subscriberSocket.SubscribeToPrefix("[MEAS]");

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

        private bool AskServer(string message, out string response)
        {
            bool success = false;
            string received = "";
            lock (_lockObject)
            {
                 success = (reqSocket.TrySendFrame(TimeSpan.FromSeconds(2), message) && reqSocket.TryReceiveFrameString(TimeSpan.FromSeconds(2), out received));
            }
            response = received;
            //_logger.LogInformation(response);
            return success;
        }

        private void TimerCallback(object? state)
        {
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

        public void StopTimer()
        {
            // Stop the timer when needed
            _timer.Dispose();
        }

        public SiPMInstrument(string InstrumentName, string ip, int controlPort, int logPort, TimeSpan period, ILogger<SiPMInstrument> logger)
        {
            _logger = logger;

            this.InstrumentName = InstrumentName;

            string reqSocIP = "tcp://" + ip + ":" + controlPort.ToString();
            string subSocIP = "tcp://" + ip + ":" + logPort.ToString();
            //reqSocket.Connect("tcp://192.168.0.45:5556");
            reqSocket.Connect(reqSocIP);

            _timer = new Timer(TimerCallback, null, TimeSpan.Zero, period); // Change the interval as needed
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

