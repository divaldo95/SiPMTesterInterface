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
        private static SerialPort? _serialPort;
        private static object _lockObject = new object();
        private AutoResetEvent _messageReceived;
        private int _timeout = 10000;

        private bool _error = false;

        public string LastLine { get; private set; } = "";

        public SerialPortHandler(IConfiguration config, string obj) : this(new SerialSettings(config, obj))
        {
        }

        public SerialPortHandler(SerialSettings settings) : this(settings.SerialPort, settings.BaudRate, settings.Timeout, settings.Enabled)
        {
        }

        public SerialPortHandler(string Port, int Baud, int Timeout = 10000, bool Enabled = false)
        {
            _messageReceived = new AutoResetEvent(false);
            _timeout = Timeout;
            if (_serialPort == null)
            {
                _serialPort = new SerialPort();
                // Set the read/write timeouts
                _serialPort.ReadTimeout = _timeout;
                _serialPort.WriteTimeout = _timeout;
                _serialPort.PortName = Port;
                _serialPort.BaudRate = Baud;
                if (Enabled)
                {
                    Start();
                }
            }
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
            }
        }

        public void Stop()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
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
                        Trace.WriteLine("Command sent to serial pulser: " + command);
                        _serialPort.DiscardInBuffer();
                        _serialPort.Write(command + System.Convert.ToChar(System.Convert.ToUInt32("0x0D", 16)));
                        LastLine = _serialPort.ReadLine();
                        Trace.WriteLine("Received: " + LastLine);

                        if (LastLine.Contains("ERROR") || LastLine.Contains("invalid")) //if returned with error, increase counter and try again
                        {
                            Trace.WriteLine("Serial port returned with error. Trying again... (" + retry_cnt + "/" + retry_num + ")");
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

