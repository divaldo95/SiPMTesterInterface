using System;
using System.Net.WebSockets;
using NetMQ;
using NetMQ.Sockets;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;
using static NetMQ.NetMQSelector;

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
        private bool needReconnect = false;

        private readonly TimeSpan Period;
        private object _lockObject = new object();
        private object _updateLock = new object();

        private Timer _timer;

        private const int RequestTimeout = 3000;
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

            //Console.WriteLine("C: Server replied OK ({0})", outString);
            retriesLeft = RequestRetries;
            expectReply = false;
            OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs(outString));
        }

        void TerminateClient(NetMQSocket client)
        {
            if (client == null || client.IsDisposed)
            {
                return;
            }
            client.Disconnect(IP);
            //client.Close();
            client.Dispose();
            GC.Collect();
        }

        RequestSocket CreateServerSocket()
        {
            bool opening = true;
            int openCount = 0;
            RequestSocket? client = null;
            Console.WriteLine("C: Connecting to server...");
            while(opening)
            {
                try
                {
                    //Handle NetMQ.NetMQException here
                    client = new RequestSocket();
                    var clientPair = new NetMQCertificate();

                    var serverPair = KeyReader.ReadKeyFiles("REQPrivate.key", "REQPublic.key", true);
                    client.Options.CurveServerKey = serverPair.PublicKey;
                    client.Options.CurveCertificate = clientPair;
                    client.Connect(IP);
                    client.Options.Linger = TimeSpan.Zero;
                    client.ReceiveReady += ClientOnReceiveReady;
                    opening = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Socket opening error: {ex.Message}");
                    openCount++;
                    if (openCount >= 10)
                    {
                        Console.WriteLine($"Socket opening limit reached");
                        throw;      
                    }
                }
            }
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
                    //Console.WriteLine("C: Sending ({0})", sequence);
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

                        //Console.WriteLine("C: No response from server, retrying...");

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
                    bool needReconnect = false;
                    Task t = new Task(() =>
                    {
                        if (needReconnect)
                        {
                            try
                            {
                                TerminateClient(reqSocket);
                                Thread.Sleep(3000);
                                reqSocket = CreateServerSocket(); //if any error happens, reopen the socket
                                needReconnect = false;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                                return;
                            }
                        }
                        try
                        {
                            AskServer(item);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                            needReconnect = true;
                        }                        
                    });
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
            try
            {
                AskServer(command);
            }
            catch (NetMQException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TerminateClient(reqSocket);
                reqSocket = CreateServerSocket();
            }
        }

        public void ChangeTimerInterval(TimeSpan period)
        {
            _timer.Change(period, period); //wait the period for the first time too
        }

        public void ChangeTimerInterval(int timeout)
        {
            _timer.Change(timeout, timeout); //wait the period for the first time too
        }

        //if an alive message arrives restart the timer
        public void Restart()
        {
            Stop();
            Start();
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

