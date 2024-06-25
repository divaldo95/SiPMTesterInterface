using System;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
    public class PSoCCommunicatorDataModel
    {
        public TemperaturesArray? Temperature { get; set; }
        public CoolerResponse? CoolerState { get; set; }

        public PSoCCommunicatorDataModel()
        {
        }

        public PSoCCommunicatorDataModel(TemperaturesArray t, CoolerResponse c)
        {
            Temperature = t;
            CoolerState = c;
        }
    }
}

