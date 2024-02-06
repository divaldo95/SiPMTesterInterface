using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using static SiPMTesterInterface.Classes.MeasurementMode;

namespace SiPMTesterInterface.Classes
{
	public class PSoCCommunicator : SerialPortHandler
	{
        private readonly ILogger<PSoCCommunicator> _logger;

		public PSoCCommunicator(string Port, int Baud, int Timeout, ILogger<PSoCCommunicator> logger) : base(Port, Baud, Timeout)
		{
            _logger = logger;
		}

        //Block - a dupla egység
        //Module - bal vagy jobb oldali 4-es modul
        // set_module,%u,%u,%u,%c,%u,%u,%u,%u
        public void SetMode(int block, int module, int SiPMNum, MeasurementModes mode, int[] brigthness)
        {
            string command = "set_module,";
            string modecmd = "";
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
            if (SiPMNum > 63 || SiPMNum < 0)
            {
                throw new Exception("SiPM num could not be higher than 63");
            }
            command += SiPMNum.ToString() + ",";
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

            //Parse lastline
            if (LastLine.Contains("ERROR") || LastLine.Contains("invalid"))
            {
                throw new Exception(LastLine);
            }
        }

        public TemperaturesArray ReadTemperatures(int block)
        {
            string command = "get_temp," + block.ToString();
            string[] temps;
            double[] tempd;
            string lastline = LastLine;
            WriteCommand(command);
            if (LastLine.Contains("ERROR") || LastLine.Contains("invalid"))
            {
                throw new Exception(LastLine);
            }
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
            //Parse lastline
            if (LastLine.Contains("ERROR") || LastLine.Contains("invalid"))
            {
                throw new Exception(LastLine);
            }
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
            //Parse lastline
            if (LastLine.Contains("ERROR") || LastLine.Contains("invalid"))
            {
                throw new Exception(LastLine);
            }
        }

        
    }
}

