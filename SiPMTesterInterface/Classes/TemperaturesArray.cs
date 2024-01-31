﻿using System;
namespace SiPMTesterInterface.Classes
{
    public class TemperaturesArray
    {
        public double[] Module1 { get; private set; } = new double[8];
        public double[] Module2 { get; private set; } = new double[8];
        public double Pulser { get; private set; }
        public double ControlTemperature { get; private set; }

        public TemperaturesArray(double[] psocRespArr)
        {
            if (psocRespArr.Length != (2*8 + 2))
            {
                throw new Exception("PSoCArray has invalid length");
            }
            for(int i = 0; i < 8; i++)
            {
                Module1[i] = psocRespArr[i];
                Module1[i + 8] = psocRespArr[i];
            }
            Pulser = psocRespArr[16];
            ControlTemperature = psocRespArr[17];

        }
    }
}

