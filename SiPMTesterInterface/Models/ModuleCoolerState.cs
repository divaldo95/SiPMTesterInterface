using System;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Models
{
	public class ModuleCoolerState
	{
        public int Block { get; set; }
        public int Module { get; set; }
        public CoolerStates ActualState { get; set; } = CoolerStates.Off;
		public bool IsTemperatureStable { get; set; } = false;
        public double CoolerTemperature { get; set; } = 0.0;
        public double PeltierVoltage { get; set; } = 0.0;
        public double PeltierCurrent { get; set; } = 0.0;
        public long Timestamp { get; set; } = 0;

        public ModuleCoolerState()
		{
		}

        public ModuleCoolerState(int block, int module, CoolerResponse resp)
        {
            if (block != resp.Block)
            {
                throw new ArgumentException($"Invalid block number ({block}) and block data ({resp.Block}) given to CoolerState");
            }
            Block = block;
            Module = module;
            Timestamp = resp.Timestamp;
            if (module == 0)
            {
                ActualState = (CoolerStates)resp.State1;
                IsTemperatureStable = resp.TempStableFlag1;
                CoolerTemperature = resp.Cooler1Temp;
                PeltierVoltage = resp.Peltier1Voltage;
                PeltierCurrent = resp.Peltier1Current;
            }
            else if (module == 1)
            {
                ActualState = (CoolerStates)resp.State2;
                IsTemperatureStable = resp.TempStableFlag2;
                CoolerTemperature = resp.Cooler2Temp;
                PeltierVoltage = resp.Peltier2Voltage;
                PeltierCurrent = resp.Peltier2Current;
            }
            else
            {
                throw new ArgumentException($"Invalid module ({module}) given to CoolerState");
            }
        }
	}
}

