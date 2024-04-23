using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using SiPMTesterInterface.Controllers;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
    public class SerialPortHandler
    {
        private SerialPort? _serialPort = null;
        private static object _lockObject = new object();
        private AutoResetEvent _messageReceived;
        private int _timeout = 10000;
        public bool Connected { get; private set; } = false;

        private readonly ILogger<SerialPortHandler> _logger;

        private bool _error = false;

        public string LastLine { get; private set; } = "";

        public SerialPortHandler(IConfiguration config, ILogger<SerialPortHandler> logger, string obj) : this(new SerialSettings(config, obj), logger)
        {
        }

        public SerialPortHandler(SerialSettings settings, ILogger<SerialPortHandler> logger) : this(logger, settings.SerialPort, settings.BaudRate, settings.Timeout, settings.Enabled, settings.AutoDetect, settings.AutoDetectString, settings.AutoDetectExpectedAnswer)
        {
        }

        public SerialPortHandler(ILogger<SerialPortHandler> logger, string Port, int Baud, int Timeout = 10000, bool Enabled = false, bool autoDetect = false, string autoDetectString = "", string autoDetectExpectedAnswer = "")
        {
            _logger = logger;
            string _Port = Port;
            if (autoDetect)
            {
                try
                {
                    _Port = SerialPortHandler.GetAutoDetectedPort(_logger, Baud, 500, autoDetectString, autoDetectExpectedAnswer);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{ex.Message}");
                }
            }

            _messageReceived = new AutoResetEvent(false);
            _timeout = Timeout;
            if (_serialPort == null)
            {
                _serialPort = new SerialPort();
                // Set the read/write timeouts
                _serialPort.ReadTimeout = _timeout;
                _serialPort.WriteTimeout = _timeout;
                _serialPort.PortName = _Port;
                _serialPort.BaudRate = Baud;
                _logger.LogInformation($"Using {_Port}");
                if (Enabled)
                {
                    Start();
                }
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
                    var serial = new SerialPortHandler(logger, port, Baud, Timeout, true, false);
                    serial.WriteCommand(""); //if the buffer is not empty, this will clear it
                    serial.WriteCommand(autoDetectString);
                    serial.Stop();
                    if (serial.LastLine.Contains(autoDetectExpectedAnswer)) //in case of non visible characters
                    {
                        return port;
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
            if (_serialPort != null && !_serialPort.IsOpen)
            {
                _serialPort.Open();
                Connected = true;
            }
        }

        public void Stop()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                Connected = false;
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
                        LastLine = _serialPort.ReadLine();
                        _logger.LogDebug("Received: " + LastLine);

                        if (LastLine.Contains("ERROR") || LastLine.Contains("invalid")) //if returned with error, increase counter and try again
                        {
                            _logger.LogDebug("Serial port returned with error. Trying again... (" + retry_cnt + "/" + retry_num + ")");
                            retry_cnt++;
                            Thread.Sleep(2000);
                        }
                        else //if returned without error then exit from while loop
                        {
                            break;
                        }
                    }
                    catch (TimeoutException exp)
                    {
                        retry_cnt++;
                        Thread.Sleep(2000);
                        continue;
                    }
                    catch (Exception e)
                    {
                        //Trace.WriteLine("Serial port error: " + e.Message);
                        _error = true;
                        throw new Exception(e.Message);
                    }
                }
                if (retry_cnt >= retry_num)
                {
                    _error = true;
                    throw new Exception("Serial port timeout");
                }
            }

        }
    }
}

