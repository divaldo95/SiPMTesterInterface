using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using static SiPMTesterInterface.Classes.MeasurementMode;
using SiPMTesterInterface.Models;
using System.Net.WebSockets;
using static SiPMTesterInterface.Classes.CoolerResponse;
using SiPMTesterInterface.Enums;
using System.Text;
using System.Globalization;
using static SiPMTesterInterface.Classes.LEDPulserData;

namespace SiPMTesterInterface.Classes
{
    public class PSoCCommuicatorDataReadEventArgs : EventArgs
    {
        public PSoCCommuicatorDataReadEventArgs() : base()
        {
            Data = new PSoCCommunicatorDataModel();
        }

        public PSoCCommuicatorDataReadEventArgs(PSoCCommunicatorDataModel d) : base()
        {
            Data = d;
        }
        public PSoCCommunicatorDataModel Data { get; set; }
    }

    public class CoolerDataReceivedEventArgs : EventArgs
    {
        public CoolerDataReceivedEventArgs() : base()
        {
            Data = new CoolerResponse();
        }

        public CoolerDataReceivedEventArgs(CoolerResponse c) : base()
        {
            Data = c;
        }
        public CoolerResponse Data { get; set; }
    }

    public class TemperatureDataReceivedEventArgs : EventArgs
    {
        public TemperatureDataReceivedEventArgs() : base()
        {
            Data = new TemperaturesArray();
        }

        public TemperatureDataReceivedEventArgs(TemperaturesArray t) : base()
        {
            Data = t;
        }
        public TemperaturesArray Data { get; set; }
    }

    public class PSoCCommunicator : SerialPortHandler
	{
        public Queue<TemperaturesArray> Temperatures { get; private set; }
        public Queue<CoolerResponse> CoolerStates { get; private set; }
        private int bufferSize = 5000; //store n number of temperatureArrays
        public TimeSpan UpdatePeriod { get; private set; } = TimeSpan.FromSeconds(5);

        private bool timerIsRunning = false;
        private Timer _timer;
        public event EventHandler<PSoCCommuicatorDataReadEventArgs> OnDataReadout;

        public List<int> ActiveBlocks { get; private set; } = new List<int>();

        public event EventHandler<CoolerDataReceivedEventArgs> OnCoolerDataReceived;
        public event EventHandler<TemperatureDataReceivedEventArgs> OnTemperatureDataReceived;

        public PSoCCommunicator(IConfiguration config, ILogger<SerialPortHandler> logger) : this(new SerialSettings(config, "Pulser"), logger)
        {
        }

        public PSoCCommunicator(SerialSettings settings, ILogger<SerialPortHandler> logger) :
            this(settings.SerialPort, settings.BaudRate, settings.Timeout, logger, settings.Enabled, settings.AutoDetect, settings.AutoDetectString, settings.AutoDetectExpectedAnswer)
        {
        }

		public PSoCCommunicator(string Port, int Baud, int Timeout, ILogger<SerialPortHandler> logger, bool Enabled, bool autoDetect, string autoDetectString, string autoDetectExpectedAnswer) :
            base(logger,Port, Baud, Timeout, Enabled, autoDetect, autoDetectString, autoDetectExpectedAnswer)
		{
            Temperatures = new Queue<TemperaturesArray>(bufferSize);
            CoolerStates = new Queue<CoolerResponse>(bufferSize);
            _timer = new Timer(TimerCallback, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            if (UpdatePeriod.Seconds > 0)
            {
                StartTimer();
            }
        }

        public void AddTemperatureArray(TemperaturesArray temperatureArray)
        {
            // If buffer is full, dequeue the oldest element
            if (Temperatures.Count == bufferSize)
            {
                Temperatures.Dequeue();
            }

            // Add the new temperature array to the buffer
            Temperatures.Enqueue(temperatureArray);
        }

        public void AddCoolerArray(CoolerResponse coolerState)
        {
            // If buffer is full, dequeue the oldest element
            if (CoolerStates.Count == bufferSize)
            {
                CoolerStates.Dequeue();
            }

            // Add the new temperature array to the buffer
            CoolerStates.Enqueue(coolerState);
        }

        public void RefreshData()
        {
            //TODO: Handle blocks properly here
            bool timeoutHappened;

            // list can be altered
            int[] activeBlocks = new int[ActiveBlocks.Count];
            ActiveBlocks.CopyTo(activeBlocks);

            try
            {
                foreach (var block in activeBlocks)
                {
                    TemperaturesArray t = ReadTemperatures(block);
                    CoolerResponse c = GetCoolerState(block);
                    AddTemperatureArray(t);
                    OnTemperatureDataReceived?.Invoke(this, new TemperatureDataReceivedEventArgs(t));
                    AddCoolerArray(c);
                    OnCoolerDataReceived?.Invoke(this, new CoolerDataReceivedEventArgs(c));
                    OnDataReadout?.Invoke(this, new PSoCCommuicatorDataReadEventArgs(new PSoCCommunicatorDataModel(t, c)));
                }
                timeoutHappened = false;
            }
            catch (TimeoutException ex)
            {
                timeoutHappened = true;
                _logger.LogWarning($"PSoC reading timeout: {ex.Message}");
            }
            catch(Exception ex)
            {
                timeoutHappened = false;
                _logger.LogDebug(ex.StackTrace);
                _logger.LogDebug(ex.Message);
            }

            try
            {
                //it can throw exceptions when attempting to restart serial communication
                TimeoutHandler(timeoutHappened); //timeout occured
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"PSoC: {ex.Message}");
            }
            
        }

        private void TimerCallback(object? state)
        {
            if (State != Enums.SerialPortState.Connected)
            {
                return;
            }
            try
            {
                RefreshData();
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Refresh error: {ex.Message}");
            }
            
        }

        public void ChangeTimerInterval(TimeSpan period)
        {
            UpdatePeriod = period;
            _timer.Change(period, period); //wait the period for the first time too
        }

        //in seconds
        public void ChangeTimerInterval(int timeout)
        {
            ChangeTimerInterval(TimeSpan.FromSeconds(timeout));
        }

        //if an alive message arrives restart the timer
        public void Restart()
        {
            StopTimer();
            StartTimer();
        }

        public void StartTimer()
        {
            if (!timerIsRunning)
            {
                ChangeTimerInterval(UpdatePeriod);
                timerIsRunning = true;
            }
            
        }

        public void StopTimer()
        {
            if (timerIsRunning)
            {
                ChangeTimerInterval(0);
                timerIsRunning = false;
            }
        }

        //Block - a dupla egység
        //Module - bal vagy jobb oldali 4-es modul
        // set_module,%u,%u,%u,%c,%u,%u,%u,%u
        public void SetMode(int block, int module, int array, int SiPMNum, MeasurementModes mode, int[] brigthness)
        {
            string command = "set_module,";
            string modecmd = "";
            int actualSiPMNum = 0;
            if (block > 1 || block < 0)
            {
                throw new Exception("Block could not be higher than 1");
            }
            command += block.ToString() + ",";
            if (module > 1 || module < 0)
            {
                throw new Exception("Module could not be higher than 1");
            }
            command += module.ToString() + ",";
            if ((SiPMNum > 15 || SiPMNum < 0) || (array > 3 || array < 0))
            {
                throw new Exception("Incorrect SiPM num or array num");
            }
            actualSiPMNum = array * 16 + SiPMNum;
            command += actualSiPMNum.ToString() + ",";
            command += MeasurementMode.GetMeasurementChar(mode);
            command += modecmd + ",";
            if (brigthness.Length != 4)
            {
                throw new Exception("Brightness array length must be 4");
            }
            foreach (var b in brigthness)
            {
                if (b > 4095 || b < 0)
                {
                    throw new Exception("Brightness could not be higher than 4095");
                }
                command += b.ToString() + ",";
            }
            command = command.Remove(command.Length - 1);
            WriteCommand(command);
        }

        public APSUDiagResponse GetAPSUStates()
        {
            string command = "get_A_psu_diag";
            WriteCommand(command);

            // Remove the "get_A_psu_diag,OK*" prefix and trailing "*\n"
            string trimmedData = LastLine.Substring("get_A_psu_diag,OK*".Length);
            trimmedData = trimmedData.Substring(0, trimmedData.Length - 2); // Remove the trailing "*\n"

            // Split the remaining data by commas
            string[] tokens = trimmedData.Split(',');

            // Create a new instance of the struct
            APSUDiagResponse diagResp = new APSUDiagResponse();

            // Assign values from the tokens
            diagResp.i2cReadErrorFlags = byte.Parse(tokens[0], System.Globalization.NumberStyles.HexNumber);
            diagResp.state = byte.Parse(tokens[1], System.Globalization.NumberStyles.HexNumber);

            for (int i = 0; i < 6; i++)
            {
                diagResp.errorFlags[i] = byte.Parse(tokens[2 + i], System.Globalization.NumberStyles.HexNumber);
            }

            for (int i = 0; i < 6; i++)
            {
                diagResp.AVoltage[i] = float.Parse(tokens[8 + i]);
            }

            for (int i = 0; i < 6; i++)
            {
                diagResp.ACurrent[i] = float.Parse(tokens[14 + i]);
            }

            for (int i = 0; i < 4; i++)
            {
                if(diagResp.GetAPSUState(i))
                {
                    if (ActiveBlocks.LastIndexOf(i) < 0) //if not in list
                    {
                        ActiveBlocks.Add(i); //add
                    }
                }
            }

            return diagResp;

        }

        private string ExtractDataFromResponse()
        {
            string lastline = LastLine;
            lastline = lastline.Remove(0, lastline.IndexOf('*') + 1); //remove everything until first *
            lastline = lastline.Remove(lastline.IndexOf('*'), 1); //remove last *
            return lastline;
        }

        public TemperaturesArray ReadTemperatures(int block)
        {
            string command = "get_temp," + block.ToString();
            string[] temps;
            double[] tempd;
            WriteCommand(command);
            string lastline = ExtractDataFromResponse();
            temps = lastline.Split(',');
            tempd = new double[temps.Length];
            for (int i = 0; i < temps.Length; i++)
            {
                tempd[i] = double.Parse(temps[i].Replace(',', '.'));
            }
            return new TemperaturesArray(block, tempd);
        }

        public CoolerResponse GetCoolerState(int block)
        {
            string command = "get_cooler_state," + block.ToString();
            WriteCommand(command);
            CoolerResponse cooler = new CoolerResponse(block, LastLine);
            //Trace.WriteLine(cooler.ToString());
            return cooler;
        }

        //set_cooler,block,module,state,target_temp,FAN_speed
        //state - on/off
        //target_temp - in °C
        //fan speed 0-100
        public void SetCooler(int block, int module, bool state, double target_temp, int fan_speed)
        {
            string command = "set_cooler,";
            if (block > 1 || block < 0)
            {
                throw new Exception("Block could not be higher than 1");
            }
            command += block.ToString() + ",";
            if (module > 1 || module < 0)
            {
                throw new Exception("Module could not be higher than 1");
            }
            command += module.ToString() + ",";
            if (state)
            {
                command += "1,";
            }
            else
            {
                command += "0,";
            }
            if (target_temp > 40 || target_temp < -50)
            {
                throw new Exception("Temperatures could not be higher than 40 or lower than -50");
            }
            command += target_temp.ToString() + ",";

            if (fan_speed > 100 || fan_speed < 0)
            {
                throw new Exception("Fan speed must be between 0 and 100");
            }
            command += fan_speed.ToString();
            WriteCommand(command);
        }

        public void EnablePSU(int block, PSUs psu, out double voltage, out double current)
        {
            string psuStr;
            switch (psu)
            {
                case PSUs.PSU_A:
                    psuStr = "A";
                    break;
                case PSUs.PSU_D:
                    psuStr = "D";
                    break;
                default:
                    throw new ArgumentException("Unknown PSU");
            }
            string command = $"enable_{psuStr}_psu,{block.ToString()}";
            try
            {
                WriteCommand(command);
                bool timerState = timerIsRunning;
                TimeSpan interval = UpdatePeriod;
                if (timerIsRunning)
                {
                    StopTimer();
                }
                if (!ActiveBlocks.Contains(block))
                {
                    ActiveBlocks.Add(block);
                }
                WriteCommand(command);
                if (timerState)
                {
                    UpdatePeriod = interval;
                    StartTimer();
                }
            }
            catch (Exception)
            {
                if (LastLine.Contains("overcurrent", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ErrorMessageReceivedException("Overcurrent detected");
                }
                else if (LastLine.Contains("overvoltage", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ErrorMessageReceivedException("Overvoltage detected");
                }
                else if (LastLine.Contains("undervoltage", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ErrorMessageReceivedException("Undervoltage detected");
                }
                else if (LastLine.Contains("powergood L", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ErrorMessageReceivedException("Power failure detected");
                }
                else if (LastLine.Contains("enable io exp read error", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ErrorMessageReceivedException("IO expander write error");
                }
                else if (LastLine.Contains("PG io exp read error", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ErrorMessageReceivedException("IO expander power failure detected");
                }
                else if (LastLine.Contains("INA226 read error", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ErrorMessageReceivedException("Unable to read INA226");
                }
                else if (LastLine.Contains("unknown error", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ErrorMessageReceivedException("Unknown error detected");
                }
            }
            
            //TODO: add custom exceptions
            string lastline = ExtractDataFromResponse();
            string[] strArray = lastline.Split(',');
            if (strArray.Length != 2)
            {
                throw new ArgumentException($"Invalid response: {lastline}");
            }
            voltage = double.Parse(strArray[0], CultureInfo.InvariantCulture);
            current = double.Parse(strArray[1], CultureInfo.InvariantCulture);
        }

        public void DisablePSU(int block, PSUs psu)
        {
            string psuStr;
            switch (psu)
            {
                case PSUs.PSU_A:
                    psuStr = "A";
                    break;
                case PSUs.PSU_D:
                    psuStr = "D";
                    break;
                default:
                    throw new ArgumentException("Unknown PSU");
            }
            string command = $"disable_{psuStr}_psu,{block.ToString()}";
            bool timerState = timerIsRunning;
            TimeSpan interval = UpdatePeriod;
            if (timerIsRunning)
            {
                StopTimer();
            }
            if (ActiveBlocks.Contains(block))
            {
                ActiveBlocks.Remove(block);
            }
            WriteCommand(command);
            if (timerState)
            {
                UpdatePeriod = interval;
                StartTimer();
            }
        }
    }
}

