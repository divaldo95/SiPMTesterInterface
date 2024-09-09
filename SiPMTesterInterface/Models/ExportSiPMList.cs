using System;
namespace SiPMTesterInterface.Models
{
	public class ExportSiPMList
	{
		public List<CurrentSiPMModel> SiPMs { get; set; } = new List<CurrentSiPMModel>();
		public ExportSiPMList()
		{
		}
	}
}

