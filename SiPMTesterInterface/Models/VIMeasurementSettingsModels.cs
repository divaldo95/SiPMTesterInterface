using System;
using System.Text;

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
        public double ExpectedRforward { get; set; }
        public double RforwardLimit { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Forward resistance expected value: {ExpectedRforward}{Environment.NewLine}");
            sb.Append($"Forward resistance limit: {RforwardLimit}{Environment.NewLine}");
            return sb.ToString();
        }
    }

    public class DarkCurrentConfig
    {
        public bool Enabled { get; set; }
        public int Iterations { get; set; }
        public double MinFirstDarkCurrent {get; set;}
        public double MaxFirstDarkCurrent { get; set; }
        public LeakageCurrent LeakageCurrent { get; set; }
        public DarkCurrent DarkCurrent { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"First dark current limits: {MinFirstDarkCurrent} < Rfd < {MaxFirstDarkCurrent} {Environment.NewLine}");
            return sb.ToString();
        }
    }
}

