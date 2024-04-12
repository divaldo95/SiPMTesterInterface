﻿using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using static SiPMTesterInterface.Classes.MeasurementMode;
using SiPMTesterInterface.Models;
using System.Net.WebSockets;

namespace SiPMTesterInterface.Classes
{
	public class PSoCCommunicator : SerialPortHandler
	{
        private Queue<TemperaturesArray> buffer;
        private int bufferSize = 5000; //store n number of temperatureArrays
        private readonly TimeSpan Period = TimeSpan.FromSeconds(10);
        private Timer _timer;

        public PSoCCommunicator(IConfiguration config) : base(config, "Pulser")
        {
            buffer = new Queue<TemperaturesArray>(bufferSize);
            _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public PSoCCommunicator(SerialSettings settings) : base(settings)
        {
            buffer = new Queue<TemperaturesArray>(bufferSize);
            _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

		public PSoCCommunicator(string Port, int Baud, int Timeout) : base(Port, Baud, Timeout)
		{
            buffer = new Queue<TemperaturesArray>(bufferSize);
            _timer = new Timer(TimerCallback, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        public void AddTemperatureArray(TemperaturesArray temperatureArray)
        {
            // If buffer is full, dequeue the oldest element
            if (buffer.Count == bufferSize)
            {
                buffer.Dequeue();
            }

            // Add the new temperature array to the buffer
            buffer.Enqueue(temperatureArray);
        }

        private void TimerCallback(object? state)
        {
            //TODO: Handle blocks properly here
            AddTemperatureArray(ReadTemperatures(0));
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
            StopTimer();
            StartTimer();
        }

        public void StartTimer()
        {
            ChangeTimerInterval(Period);
        }

        public void StopTimer()
        {
            ChangeTimerInterval(Timeout.Infinite);
        }

        public bool IsPulser()
        {
            string command = "get_instrument_name";
            WriteCommand(command);
            string lastline = LastLine;
            if (lastline.Contains("OK") && lastline.Contains("Pulser"))
            {
                return true;
            }
            else
            {
                return false;
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

        public TemperaturesArray ReadTemperatures(int block)
        {
            string command = "get_temp," + block.ToString();
            string[] temps;
            double[] tempd;
            WriteCommand(command);
            string lastline = LastLine;
            lastline = lastline.Remove(0, LastLine.IndexOf('*') + 1); //remove everything until first *
            lastline = lastline.Remove(lastline.IndexOf('*'), 1); //remove last *
            temps = lastline.Split(',');
            tempd = new double[temps.Length];
            for (int i = 0; i < temps.Length; i++)
            {
                tempd[i] = double.Parse(temps[i].Replace(',', '.'));
            }
            return new TemperaturesArray(tempd);
        }

        public Cooler GetCoolerState(int block)
        {
            string command = "get_cooler_state," + block.ToString();
            string[] temps;
            double[] tempd;
            WriteCommand(command);
            Cooler cooler = new Cooler(LastLine);
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

        public bool IsPulserOpened()
        {
            string command = "get_instrument_name";
            WriteCommand(command);
            Console.WriteLine($"Received string: {LastLine}");
            if (LastLine.Contains("Pulser") && LastLine.Contains("OK"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

