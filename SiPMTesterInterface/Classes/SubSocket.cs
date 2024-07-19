using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using SiPMTesterInterface.Helpers;

namespace SiPMTesterInterface.Classes
{
    public class MessageReceivedIntervalElapsedEventArgs : EventArgs
    {
        public MessageReceivedIntervalElapsedEventArgs() : base()
        {
        }
    }

    public class LogMessageReceivedEventArgs : EventArgs
    {
        public LogMessageReceivedEventArgs(string msg) : base()
        {
            Message = msg;
        }
        public string Message { get; set; }
    }

    public class MeasurementMessageReceivedEventArgs : EventArgs
    {
        public MeasurementMessageReceivedEventArgs(string msg) : base()
        {
            Message = msg;
        }
        public string Message { get; set; }
    }

    public class StatusMessageReceivedEventArgs : EventArgs
    {
        public StatusMessageReceivedEventArgs(string msg) : base()
        {
            Message = msg;
        }
        public string Message { get; set; }
    }

    public class SubSocket
    {
        private NetMQPoller poller;
        private string IP = "";

        private Timer _timer;
        public TimeSpan ReceiveIntervalTimeout = Timeout.InfiniteTimeSpan;

        private readonly object bufferLock = new object();
        private readonly ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Task messageProcessingTask;

        public event EventHandler<MessageReceivedIntervalElapsedEventArgs> OnMessageReceivedIntervalElapsed;
        public event EventHandler<MeasurementMessageReceivedEventArgs> OnMeasurementMessageReceived;
        public event EventHandler<LogMessageReceivedEventArgs> OnLogMessageReceived;
        public event EventHandler<StatusMessageReceivedEventArgs> OnStatusMessageReceived;

        public void ChangeTimerInterval(TimeSpan period)
        {
            ReceiveIntervalTimeout = period;
            _timer.Change(ReceiveIntervalTimeout, ReceiveIntervalTimeout); //wait the period for the first time too
        }

        private void ChangeTimerInterval(int timeout)
        {
            _timer.Change(timeout, timeout); //wait the period for the first time too
        }

        public void Start()
        {
            //StartSubscriber(subSocIP);
            Task subscriberTask = Task.Run(() => StartSubscriber(IP));
            messageProcessingTask = Task.Run(() => ProcessMessages(cancellationTokenSource.Token));
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            if (poller != null)
            {
                poller.StopAsync();
            }
        }

        public void StartSubscriber(string publisherAddress)
        {
            using (var subscriberSocket = new SubscriberSocket())
            {
                var clientPair = new NetMQCertificate();

                var serverPair = KeyReader.ReadKeyFiles("PUBPrivate.key", "PUBPublic.key", true);
                subscriberSocket.Options.CurveServerKey = serverPair.PublicKey;
                subscriberSocket.Options.CurveCertificate = clientPair;

                subscriberSocket.Connect(publisherAddress);
                subscriberSocket.Subscribe("[LOG]");
                subscriberSocket.Subscribe("[MEAS]");
                subscriberSocket.Subscribe("[STATUS]");
                using (var poller = new NetMQPoller { subscriberSocket })
                {
                    subscriberSocket.ReceiveReady += (sender, args) =>
                    {
                        lock (bufferLock)
                        {
                            // Receive the message
                            var message = subscriberSocket.ReceiveFrameString();

                            // Enqueue the message for processing
                            messageQueue.Enqueue(message);
                            ChangeTimerInterval(ReceiveIntervalTimeout); //restart timer if needed
                        }
                    };

                    poller.Run();
                }
            }
        }

        private void ProcessMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (messageQueue.TryDequeue(out var message))
                {
                    // Process the dequeued message
                    if (message.Contains("[LOG]"))
                    {
                        OnLogMessageReceived?.Invoke(this, new LogMessageReceivedEventArgs(message.Remove(0, 5)));
                    }
                    else if (message.Contains("[MEAS]"))
                    {
                        OnMeasurementMessageReceived?.Invoke(this, new MeasurementMessageReceivedEventArgs(message.Remove(0, 6)));
                    }
                    else if (message.Contains("[STATUS]"))
                    {
                        OnStatusMessageReceived?.Invoke(this, new StatusMessageReceivedEventArgs(message.Remove(0, 8)));
                    }
                }
                else
                {
                    // If there are no messages, sleep for a short while to avoid busy-waiting
                    Thread.Sleep(5);
                }
            }
        }

        private void TimerCallback(object? state)
        {
            OnMessageReceivedIntervalElapsed?.Invoke(this, new MessageReceivedIntervalElapsedEventArgs());
        }

        public SubSocket(string ip)
        {
            IP = ip;
            _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }
    }
}