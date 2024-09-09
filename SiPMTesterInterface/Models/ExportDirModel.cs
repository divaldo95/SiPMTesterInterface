using System;
namespace SiPMTesterInterface.Models
{
	public class ExportDirModel
	{
		public string ExportDir { get; set; }

		public ExportDirModel(string dir)
		{
			ExportDir = dir;
		}
	}
}

