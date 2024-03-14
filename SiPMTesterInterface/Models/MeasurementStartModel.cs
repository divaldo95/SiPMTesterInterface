using System;
using System.Collections.Generic;

namespace SiPMTesterInterface.Models
{
    public class SiPM
    {
        public int DMMResistance { get; set; }
        public int IV { get; set; }
        public int SPS { get; set; }
        public int SPSVoltagesIsOffsets { get; set; }
        public List<double> IVVoltages { get; set; } = new List<double>();
        public List<double> SPSVoltages { get; set; } = new List<double>();
    }

    public class Array
    {
        public string Barcode { get; set; } = "";
        public List<SiPM> SiPMs { get; set; } = new List<SiPM>();
    }

    public class Module
    {
        public List<Array> Arrays { get; set; } = new List<Array>();
    }

    public class Block
    {
        public List<Module> Modules { get; set; } = new List<Module>();
    }

    public class MeasurementStartModel
    {
        public bool MeasureDMMResistance { get; set; } = false;
        public DMMResistanceModel DMMResistance { get; set; } = new DMMResistanceModel();
        public int IV { get; set; }
        public int SPS { get; set; }
        public int SPSVoltagesIsOffsets { get; set; }
        public List<double> IVVoltages { get; set; } = new List<double>();
        public List<double> SPSVoltages { get; set; } = new List<double>();
        public List<Block> Blocks { get; set; } = new List<Block>();
    }
}

