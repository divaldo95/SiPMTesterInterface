using System;
namespace SiPMTesterInterface.Models
{
	public class IVMeasurementHubUpdate
	{
        public bool isOK { get; set; } = false;
        public double breakdownVoltage { get; set; } = 0.0;
        public long startTimestamp { get; set; } = 0;
        public long endTimestamp { get; set; } = 0;
        public IVMeasurementHubUpdate()
		{
            
        }
        public IVMeasurementHubUpdate(bool ok, double vbr, long start, long end)
        {
            isOK = ok;
            breakdownVoltage = vbr;
            startTimestamp = start;
            endTimestamp = end;
        }
    }
}

