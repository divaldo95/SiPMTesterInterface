using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using SiPMTesterInterface.Controllers;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
    public class SerialTimeoutLimitReachedException : Exception
    {
        public SerialTimeoutLimitReachedException()
        {
        }

        public SerialTimeoutLimitReachedException(string message)
            : base(message)
        {
        }

        public SerialTimeoutLimitReachedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class ErrorMessageReceivedException : Exception
    {
        public ErrorMessageReceivedException()
        {
        }

        public ErrorMessageReceivedException(string message)
            : base(message)
        {
        }

        public ErrorMessageReceivedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class SerialErrorMessageReceivedEventArgs : EventArgs
    {
        //It can be error or invalid
        public SerialErrorMessageReceivedEventArgs(string message, bool isError) : base()
        {
            IsError = isError;
            ReceivedMessage = message;
        }
        public bool IsError { get; set; }
        public string ReceivedMessage { get; set; }
    }

    public class SerialConnectionStateChangedEventArgs : EventArgs
    {
        public SerialConnectionStateChangedEventArgs() : base()
        {
        }

        public SerialConnectionStateChangedEventArgs(SerialPortState s, SerialPortState p) : base()
        {
            Previous = p;
            State = s;
        }
        public SerialPortState Previous { get; set; } = SerialPortState.Disconnected;
        public SerialPortState State { get; set; } = SerialPortState.Disconnected;
    }

    public class SerialPortHandler
    {
        private SerialPort? _serialPort = null;
        string _Port = "";
        int _Baud = 115200;
        int _Timeout = 10000;

        bool _AutoDetect = false;
        string _AutoDetectString = "";
        string _AutoDetectExpectedAnswer = "";
        private static object _lockObject = new object();
        private AutoResetEvent _messageReceived;
        private SerialPortState _state = SerialPortState.Disabled;
        public SerialPortState State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (value != _state)
                {
                    SerialConnectionStateChangedEventArgs eventArgs = new SerialConnectionStateChangedEventArgs();
                    eventArgs.Previous = _state;
                    _state = value;
                    eventArgs.State = _state;
                    OnSerialStateChanged?.Invoke(this, eventArgs);
                }
                _state = value;
            }
        }

        private int TimeoutCounter { get; set; } = 0;
        public int MaxTimeoutCount { get; set; } = 5;
        protected readonly ILogger<SerialPortHandler> _logger;

        public bool Enabled { get; protected set; } = false;
        public bool Initialized { get; protected set; } = false;

        public string LastLine { get; private set; } = "";
        public event EventHandler<SerialConnectionStateChangedEventArgs> OnSerialStateChanged;
        public event EventHandler<SerialErrorMessageReceivedEventArgs> OnSerialErrorMessageReceived;

        public SerialPortHandler(IConfiguration config, ILogger<SerialPortHandler> logger, string obj) : this(new SerialSettings(config, obj), logger)
        {
        }

        public SerialPortHandler(SerialSettings settings, ILogger<SerialPortHandler> logger) : this(logger, settings.SerialPort, settings.BaudRate, settings.Timeout, settings.Enabled, settings.AutoDetect, settings.AutoDetectString, settings.AutoDetectExpectedAnswer)
        {
        }

        public SerialPortHandler(ILogger<SerialPortHandler> logger, string Port, int Baud, int Timeout = 10000, bool Enabled = false, bool autoDetect = false, string autoDetectString = "", string autoDetectExpectedAnswer = "")
        {
            this.Enabled = Enabled;
            _logger = logger;
            _Port = Port;
            _Baud = Baud;
            _AutoDetect = autoDetect;
            _AutoDetectString = autoDetectString;
            _AutoDetectExpectedAnswer = autoDetectExpectedAnswer;
            _Timeout = Timeout;
        }

        public void Init()
        {
            TimeoutCounter = 0;
            _messageReceived = new AutoResetEvent(false);
            if (_serialPort == null)
            {
                _serialPort = new SerialPort();
                // Set the read/write timeouts
                _serialPort.ReadTimeout = _Timeout;
                _serialPort.WriteTimeout = _Timeout;
                _serialPort.PortName = _Port;
                _serialPort.BaudRate = _Baud;
                _logger.LogInformation($"Using {_Port}");
                Initialized = true;
            }
        }

        public static string GetAutoDetectedPort(ILogger<SerialPortHandler> logger, int Baud, int Timeout = 500, string autoDetectString = "", string autoDetectExpectedAnswer = "")
        {
            if (String.IsNullOrWhiteSpace(autoDetectString) || String.IsNullOrEmpty(autoDetectString))
            {
                throw new FormatException($"Auto detection string (\'{autoDetectString}\') can not be empty, whitespace or null.");
            }

            string[] ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                try
                {
                    string received = "";
                    var serial = new SerialPortHandler(logger, port, Baud, Timeout, true, false, autoDetectString, autoDetectExpectedAnswer);
                    serial.Init();
                    serial.Start();
                    //serial.WriteCommand("empty"); //if the buffer is not empty, this will clear it
                    serial.WriteCommand(autoDetectString);
                    received = serial.LastLine;
                    serial.Stop();
                    serial.Close();

                    if (received.Contains(autoDetectExpectedAnswer, StringComparison.CurrentCultureIgnoreCase)) //in case of non visible characters, case insesitive
                    {
                        return port;
                    }
                    else
                    {
                        Console.WriteLine($"Device ({port}) not detected {Environment.NewLine}Expected:{autoDetectExpectedAnswer} {Environment.NewLine}Received: {received}. {Environment.NewLine}Try to find the appropriate port manually");
                        Console.WriteLine($"AutoDetectString: {autoDetectString}");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Auto detect error: {ex.Message}");
                }
            }
            throw new SystemException($"Device not detected (Expected answer was {autoDetectExpectedAnswer}), try to find the appropriate port manually");
        }

        public void MessageReceived(object sender, SerialDataReceivedEventArgs e)
        {
            this._messageReceived.Set();
        }

        public void Start()
        {
            if (!Enabled)
            {
                throw new MethodAccessException("Device not enabled, can not start");
            }
            if (!Initialized)
            {
                throw new MethodAccessException("Device not intialized, can not start");
            }
            if (_serialPort != null && !_serialPort.IsOpen)
            {
                _serialPort.Open();
                State = SerialPortState.Connected;
            }
        }

        public void Stop()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                State = SerialPortState.Disconnected;
                Initialized = false;
            }
        }

        public void Close()
        {
            if (_serialPort != null)
            {
                _serialPort.Dispose();
            }
        }

        public void Restart()
        {
            Stop();
            Close();
            Init();
            Start();
        }

        protected void TimeoutHandler(bool timeoutHappened = false)
        {
            if (timeoutHappened)
            {
                TimeoutCounter++;
            }
            else
            {
                TimeoutCounter = 0;
            }

            //Try to restart serial communication
            if (TimeoutCounter >= MaxTimeoutCount)
            {
                throw new SerialTimeoutLimitReachedException($"Timeout limit reached {TimeoutCounter}/{MaxTimeoutCount}");
            }
        }

        protected void WriteCommand(string command)
        {
            lock (_lockObject)
            {
                if (_serialPort == null)
                {
                    return;
                }
                if (!_serialPort.IsOpen)
                {
                    return;
                }
                string errorMessage = "";
                int retry_cnt = 0;
                int retry_num = 3;
                while (retry_cnt < retry_num)
                {
                    try
                    {
                        Thread.Sleep(100);
                        _logger.LogDebug("Command sent to serial pulser: " + command);
                        _serialPort.DiscardInBuffer();
                        _serialPort.Write(command + System.Convert.ToChar(System.Convert.ToUInt32("0x0D", 16)));
                        LastLine = _serialPort.ReadLine(); //Try with read
                        _logger.LogDebug("Received: " + LastLine);

                        if (LastLine.Contains("ERROR", StringComparison.CurrentCultureIgnoreCase) || LastLine.Contains("invalid", StringComparison.CurrentCultureIgnoreCase)) //if returned with error, increase counter and try again
                        {
                            string lowerCaseResp = LastLine.ToLower();
                            int indexError = lowerCaseResp.IndexOf("error");
                            int indexInvalid = lowerCaseResp.IndexOf("invalid");
                            if (indexError < 0 && indexInvalid < 0)
                            {
                                _logger.LogDebug("Could not extract data from responses");
                                errorMessage = LastLine;
                            }
                            else
                            {
                                int usedIndex = (indexError != -1 ? indexError : indexInvalid);
                                errorMessage = LastLine.Substring(usedIndex);
                            }
                            _logger.LogDebug("Serial port returned with error. Trying again... (" + retry_cnt + "/" + retry_num + ")");
                            retry_cnt++;
                            if (retry_cnt >= retry_num)
                            {
                                // OnSerialErrorMessageReceived.Invoke(this, new SerialErrorMessageReceivedEventArgs(errorMessage, indexError >= 0));
                                throw new ErrorMessageReceivedException(errorMessage);
                            }
                            Thread.Sleep(2000);
                        }
                        else //if returned without error then exit from while loop
                        {
                            TimeoutHandler(false);
                            break;
                        }
                    }
                    catch (TimeoutException exp)
                    {
                        retry_cnt++;
                        TimeoutHandler(true);
                        Thread.Sleep(2000);
                        continue;
                    }
                    catch (Exception e)
                    {
                        //Trace.WriteLine("Serial port error: " + e.Message);
                        throw;
                    }
                }
            }

        }
    }
}

