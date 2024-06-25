using System;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
	public class CoolerStateHandler
	{
        public int BlockNum { get; set; } = 0;
        public int ModuleNum { get; set; } = 0;
        public CoolerSettingsModel[] CoolerSettings { get; set; }

        public CoolerStateHandler(int blocks, int modules)
		{
            BlockNum = blocks;
            ModuleNum = modules;
            CoolerSettings = new CoolerSettingsModel[blocks * modules];
            for (int i = 0; i < CoolerSettings.Count(); i++)
            {
                CoolerSettings[i] = new CoolerSettingsModel();
                CoolerSettings[i].Block = i / modules;
                CoolerSettings[i].Module = i % modules;
            }
        }

        public int GetIndex(int block, int module)
        {
            if (block >= BlockNum || block < 0 || module >= ModuleNum || module < 0)
            {
                throw new ArgumentException($"Invalid block or module number. Actual block: {block}, Max: {BlockNum}, Actual module: {module}, Max: {ModuleNum}");
            }
            return block * ModuleNum + module;
        }

        public CoolerSettingsModel GetCoolerSettings(int block, int module)
        {
            int index = GetIndex(block, module);
            return CoolerSettings[index];
        }

        public void SetCoolerSettings(CoolerSettingsModel settings)
        {
            int index = GetIndex(settings.Block, settings.Module);
            CoolerSettings[index].Enabled = settings.Enabled;
            CoolerSettings[index].TargetTemperature = settings.TargetTemperature;
            CoolerSettings[index].FanSpeed = settings.FanSpeed;
        }

        public void SetModuleCoolerState(int block, int module, ModuleCoolerState state)
        {
            int index = GetIndex(block, module);
            CoolerSettings[index].State = state;
        }

        public void SetModuleCoolerState(ModuleCoolerState state)
        {
            int index = GetIndex(state.Block, state.Module);
            CoolerSettings[index].State = state;
        }

        public void SetModuleTemperatures(TemperaturesArray temps)
        {
            int index = GetIndex(temps.Block, 0);
            CoolerSettings[index].Temperatures = temps.Module1;
            index = GetIndex(temps.Block, 1);
            CoolerSettings[index].Temperatures = temps.Module2;
        }
    }
}

