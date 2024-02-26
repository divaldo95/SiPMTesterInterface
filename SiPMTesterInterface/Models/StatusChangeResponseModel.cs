using System;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Models
{
	public class StatusChangeResponseModel
	{
		public MeasurementState State { get; set; } = MeasurementState.Unknown;
	}
}

