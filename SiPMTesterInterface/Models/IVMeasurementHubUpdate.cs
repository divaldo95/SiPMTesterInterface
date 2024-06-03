using System;
namespace SiPMTesterInterface.Models
{
    public class IVTimes
    {
        public long startTimestamp { get; set; } = 0;
        public long endTimestamp { get; set; } = 0;

        public IVTimes()
        {
        }

        public IVTimes(long start, long end)
        {
            startTimestamp = start;
            endTimestamp = end;
        }
    }

	public class IVMeasurementHubUpdate
	{
        public IVTimes IVTimes { get; set; } = new IVTimes();
        public IVAnalysationResult IVAnalysationResult { get; set; } = new IVAnalysationResult();
        
        public IVMeasurementHubUpdate()
		{
            
        }
        public IVMeasurementHubUpdate(IVAnalysationResult res, IVTimes times)
        {
            IVTimes = times;
            IVAnalysationResult = res;
        }
    }
}

