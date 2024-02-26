using System;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
	public class MeasurementStartResponseModel
	{
		public MeasurementIdentifier Identifier;
		public bool Successful { get; set; }
		public string ErrorMessage { get; set; }
		public string AdditionalInformation { get; set; }
	}
}

