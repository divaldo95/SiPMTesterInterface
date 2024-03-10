using System;
namespace SiPMTesterInterface.Models
{
	public class AllStatusModel
	{
		public StatusModel NIMachineStatus { get; set; } = new StatusModel();
		public StatusModel HVPSUStatus { get; set; } = new StatusModel();
		public StatusModel PulserStatus { get; set; } = new StatusModel();
	}
}

