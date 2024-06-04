using System;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
	public class CoolerStateHandler
	{
        public CoolerSettingsModel[] coolerSettings = new CoolerSettingsModel[2 * 2];

        public CoolerStateHandler()
		{
            for (int i = 0; i < coolerSettings.Count(); i++)
            {
                coolerSettings[i] = new CoolerSettingsModel();
                coolerSettings[i].Block = i / 2;
                coolerSettings[i].Module = i % 2;
            }
        }

        public CoolerSettingsModel GetCoolerSettings(int block, int module)
        {
            if (block > 1 || block < 0 || module > 1 || module < 0)
            {
                throw new ArgumentException("Invalid block or module number");
            }
            int index = block * 2 + module;
            return coolerSettings[index];
        }

        public void SetCoolerSettings(CoolerSettingsModel settings)
        {
            if (settings.Block > 1 || settings.Block < 0 || settings.Module > 1 || settings.Module < 0)
            {
                throw new ArgumentException("Invalid block or module number");
            }
            int index = settings.Block * 2 + settings.Module;
            coolerSettings[index] = settings;
        }
    }
}

