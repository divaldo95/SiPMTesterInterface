using System;
namespace SiPMTesterInterface.Models
{
	public class MainViewModel
	{
        public SiPMModel SelectedSiPMs { get; set; }
        public IVModel IV { get; set; }
        public SPSModel SPS { get; set; }
        public ResultsModel Results { get; set; }
    }
}

