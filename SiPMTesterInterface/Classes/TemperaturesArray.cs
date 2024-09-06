using System;
using System.Text;
using SiPMTesterInterface.Helpers;

namespace SiPMTesterInterface.Classes
{
    public class TemperaturesArray
    {
        public int Block { get; set; }
        public double[] Module1 { get; set; } = new double[8];
        public double[] Module2 { get; set; } = new double[8];
        public double Pulser { get; set; }
        public double ControlTemperature { get; set; }
        public long Timestamp { get; set; }

        public TemperaturesArray()
        {

        }

        // Return true if all the temperatures are above -100 (values less than -100 means readout or other error)
        public bool Validate()
        {
            bool isValid = Validate(0) || Validate(1);
            return isValid;
        }

        public bool Validate(int module)
        {
            if (module == 0)
            {
                return Module1.All(t => t > -100);
            }
            else if (module == 1)
            {
                return Module2.All(t => t > -100);
            }
            else
            {
                return false;
            }
        }

        public TemperaturesArray(int block, double[] psocRespArr)
        {
            Block = block;
            if (psocRespArr.Length != (2*8 + 2))
            {
                throw new Exception("PSoCArray has invalid length");
            }
            for(int i = 0; i < 8; i++)
            {
                Module1[i] = psocRespArr[i];
                Module2[i] = psocRespArr[i+8];
            }
            Pulser = psocRespArr[16];
            ControlTemperature = psocRespArr[17];
            Timestamp = TimestampHelper.GetUTCTimestamp();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Module1: ");
            sb.AppendLine(string.Join(", ", Module1));
            sb.Append("Module2: ");
            sb.AppendLine(string.Join(", ", Module2));
            return sb.ToString();
        }
    }
}

