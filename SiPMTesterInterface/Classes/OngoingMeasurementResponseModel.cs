using System;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Classes
{
	public class OngoingMeasurementResponseModel
	{
        public MeasurementType MeasurementType { get; set; }
        public object CurrentMeasurement { get; set; }
        public long StartTimestamp { get; set; }
        public long CurrentTimestamp { get; set; }
        public string Message { get; set; }

        public OngoingMeasurementResponseModel()
		{
		}
	}
}

