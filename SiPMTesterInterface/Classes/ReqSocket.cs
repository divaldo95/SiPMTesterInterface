using System;
using System.Net.WebSockets;
using NetMQ;
using NetMQ.Sockets;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;

namespace SiPMTesterInterface.Classes
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(string msg) : base()
        {
            Message = msg;
        }
        public string Message { get; set; } = "";
    }

    public class MessageReceiveFailEventArgs : EventArgs
    {
        public MessageReceiveFailEventArgs(string msg) : base()
        {
            Message = msg;
        }
        public string Message { get; set; } = "";
    }

    public class ReqSocket
	{
		public string IP { get; private set; }
        private RequestSocket reqSocket;

        private readonly TimeSpan Period;
        private object _lockObject = new object();
        private object _updateLock = new object();

        private Timer _timer;

        private const int RequestTimeout = 1000;
        private const int RequestRetries = 3;

        private int sequence = 0;
        private bool expectReply = true;
        private int retriesLeft = RequestRetries;
        private string outString = "";

        private List<string> queryMsgs = new List<string>();

        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        public event EventHandler<MessageReceiveFailEventArgs> OnMessageReceiveFail;

        void ClientOnReceiveReady(object? sender, NetMQSocketEventArgs args)
        {
            outString = args.Socket.ReceiveFrameString();
            //string strReply = Encoding.Unicode.GetString(reply);

            Console.WriteLine("C: Server replied OK ({0})", outString);
            retriesLeft = RequestRetries;
            expectReply = false;
            OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs(outString));
        }

        void TerminateClient(NetMQSocket client)
        {
            client.Disconnect(IP);
            client.Close();
        }

        RequestSocket CreateServerSocket()
        {
            Console.WriteLine("C: Connecting to server...");

            var client = new RequestSocket();
            var clientPair = new NetMQCertificate();

            var serverPair = KeyReader.ReadKeyFiles("REQPrivate.key", "REQPublic.key", true);
            client.Options.CurveServerKey = serverPair.PublicKey;
            client.Options.CurveCertificate = clientPair;
            client.Connect(IP);
            client.Options.Linger = TimeSpan.Zero;
            client.ReceiveReady += ClientOnReceiveReady;
            return client;
        }

        protected bool AskServer(string message)
        {
            bool success = false;
            lock (_lockObject)
            {
                //success = (reqSocket.TrySendFrame(TimeSpan.FromMilliseconds(5000), message) && reqSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(800), out received));
                while (retriesLeft > 0)
                {
                    sequence++;
                    Stop(); //stop, because it can take more time than period set
                    Console.WriteLine("C: Sending ({0})", sequence);
                    reqSocket.SendFrame(message);
                    expectReply = true;

                    while (expectReply)
                    {
                        success = reqSocket.Poll(TimeSpan.FromMilliseconds(RequestTimeout));

                        if (success)
                        {
                            Start(); //start when query is done
                            break;
                        }
                        retriesLeft--;

                        if (retriesLeft == 0)
                        {
                            reqSocket = CreateServerSocket();
                            Start(); //start even when error happens, client can become alive until next try
                            Console.WriteLine("C: Server seems to be offline, abandoning");
                            OnMessageReceiveFail?.Invoke(this, new MessageReceiveFailEventArgs(message));
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

            //_logger.LogInformation(response);
            return success;
        }

        private void TimerCallback(object? state)
        {
            lock (_updateLock)
            {
                foreach (var item in queryMsgs)
                {
                    //AskServer(item);
                    Task t = new Task(() => AskServer(item));
                    // Start the task.
                    t.Start();
                }
            }
        }

        public void AddQueryMessage(string message)
        {
            if (!queryMsgs.Contains(message))
            {
                queryMsgs.Add(message);
            }
        }

        public void RemoveQueryMessage(string message)
        {
            //if in any case the message is duplicated
            while (queryMsgs.Contains(message))
            {
                queryMsgs.Remove(message);
            }
        }

        //Run command directly
        public void RunCommand(string command)
        {
            AskServer(command);
        }

        public void ChangeTimerInterval(TimeSpan period)
        {
            _timer.Change(period, period); //wait the period for the first time too
        }

        public void ChangeTimerInterval(int timeout)
        {
            _timer.Change(timeout, timeout); //wait the period for the first time too
        }

        public void Start()
        {
            ChangeTimerInterval(Period);
        }

        public void Stop()
        {
            ChangeTimerInterval(Timeout.Infinite);
        }

        public ReqSocket(string ip, TimeSpan period)
		{
			IP = ip;
            reqSocket = CreateServerSocket();
            //_timer = new Timer(TimerCallback, null, TimeSpan.Zero, Period); // Change the interval as needed
            Period = period;
            _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite); //init timer, but keep it stopped, use ChangeTimerInterval() to start
        }
	}
}

