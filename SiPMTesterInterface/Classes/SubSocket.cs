using System;
using NetMQ;
using NetMQ.Sockets;

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

    public class SubSocket
	{
        private NetMQPoller poller;
        private string IP = "";

        private Timer _timer;
        public TimeSpan ReceiveIntervalTimeout = Timeout.InfiniteTimeSpan;

        private readonly object bufferLock = new object();

        public event EventHandler<MessageReceivedIntervalElapsedEventArgs> OnMessageReceivedIntervalElapsed;
        public event EventHandler<MeasurementMessageReceivedEventArgs> OnMeasurementMessageReceived;
        public event EventHandler<LogMessageReceivedEventArgs> OnLogMessageReceived;

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
        }

        public void Stop()
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

                            //Event
                            if (message.Contains("[LOG]"))
                            {
                                OnLogMessageReceived?.Invoke(this, new LogMessageReceivedEventArgs(message));
                            }
                            else if (message.Contains("[MEAS]"))
                            {
                                OnMeasurementMessageReceived?.Invoke(this, new MeasurementMessageReceivedEventArgs(message));
                            }
                            ChangeTimerInterval(ReceiveIntervalTimeout); //restart timer if needed
                        }
                    };

                    poller.Run();
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

