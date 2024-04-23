using System;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
    public class PSoCCommunicatorDataModel
    {
        public TemperaturesArray? Temperature { get; set; }
        public Cooler? CoolerState { get; set; }

        public PSoCCommunicatorDataModel()
        {
        }

        public PSoCCommunicatorDataModel(TemperaturesArray t, Cooler c)
        {
            Temperature = t;
            CoolerState = c;
        }
    }
}

