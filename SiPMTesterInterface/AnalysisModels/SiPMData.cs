using System;
using System.Runtime.InteropServices;

namespace SiPMTesterInterface.AnalysisModels
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SiPMData
    {
        public IntPtr voltages;
        public IntPtr currents;
        public UIntPtr dataPoints;
        public double preTemp;
        public double postTemp;
        public ulong timestamp;
    }
}

