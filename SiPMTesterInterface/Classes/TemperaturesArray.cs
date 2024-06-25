using System;
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
    }
}

