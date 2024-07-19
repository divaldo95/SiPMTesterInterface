using System;
namespace SiPMTesterInterface.Models
{
    public class LeakageCurrent
    {
        public double FirstVoltageOffset { get; set; }
        public double FirstVoltageRange { get; set; }
        public double FirstCurrentLimit { get; set; }
        public double FirstCurrentLimitRange { get; set; }
        public double SecondVoltageOffset { get; set; }
        public double SecondVoltageRange { get; set; }
        public double SecondCurrentLimit { get; set; }
        public double SecondCurrentLimitRange { get; set; }
    }

    public class DarkCurrent
    {
        public double FirstVoltageOffset { get; set; }
        public double FirstVoltageRange { get; set; }
        public double FirstCurrentLimit { get; set; }
        public double FirstCurrentLimitRange { get; set; }
        public double SecondVoltageOffset { get; set; }
        public double SecondVoltageRange { get; set; }
        public double SecondCurrentLimit { get; set; }
        public double SecondCurrentLimitRange { get; set; }
    }

    public class ForwardResistanceConfig
    {
        public bool Enabled { get; set; }
        public int Iterations { get; set; }
        public double FirstVoltage { get; set; }
        public double FirstVoltageRange { get; set; }
        public double FirstCurrentLimit { get; set; }
        public double FirstCurrentLimitRange { get; set; }
        public double SecondVoltage { get; set; }
        public double SecondVoltageRange { get; set; }
        public double SecondCurrentLimit { get; set; }
        public double SecondCurrentLimitRange { get; set; }
    }

    public class DarkCurrentConfig
    {
        public bool Enabled { get; set; }
        public int Iterations { get; set; }
        public LeakageCurrent LeakageCurrent { get; set; }
        public DarkCurrent DarkCurrent { get; set; }
    }
}

