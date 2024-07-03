using System;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
    public class CoolerTemperatureStabilizationChangedEventArgs : EventArgs
    {
        public CoolerTemperatureStabilizationChangedEventArgs() : base()
        {
        }

        public CoolerTemperatureStabilizationChangedEventArgs(int block, int module, bool prev, bool curr) : base()
        {
            Block = block;
            Module = module;
            PreviousState = prev;
            CurrentState = curr;
        }
        public bool PreviousState { get; set; } = false;
        public bool CurrentState { get; set; } = false;
        public int Block { get; set; } = 0;
        public int Module { get; set; } = 0;
    }

    public class CoolerFailEventArgs : EventArgs
    {
        public CoolerFailEventArgs() : base()
        {
        }

        public CoolerFailEventArgs(int block, int module, CoolerStates prev, CoolerStates curr) : base()
        {
            Block = block;
            Module = module;
            PreviousState = prev;
            CurrentState = curr;
        }
        public CoolerStates PreviousState { get; set; } = CoolerStates.Off;
        public CoolerStates CurrentState { get; set; } = CoolerStates.Off;
        public int Block { get; set; } = 0;
        public int Module { get; set; } = 0;
    }

    public class CoolerStateHandler
	{
        public int BlockNum { get; set; } = 0;
        public int ModuleNum { get; set; } = 0;
        public CoolerSettingsModel[] CoolerSettings { get; set; }

        public bool[] APSUsState { get; set; }
        public bool[] APSUsStateOverrideByUser { get; set; }

        public event EventHandler<CoolerTemperatureStabilizationChangedEventArgs> OnCoolerTemperatureStabilizationChanged;
        public event EventHandler<CoolerFailEventArgs> OnCoolerFail;

        public CoolerStateHandler(int blocks, int modules)
		{
            BlockNum = blocks;
            ModuleNum = modules;
            APSUsStateOverrideByUser = new bool[BlockNum];
            APSUsState = new bool[BlockNum]; 
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
                throw new ArgumentException($"Invalid block or module number. Actual block: {block}, Max: {BlockNum - 1}, Actual module: {module}, Max: {ModuleNum - 1}");
            }
            return block * ModuleNum + module;
        }

        public void SetAPSUStateOverride(int block, bool isOverridden)
        {
            if (block >= BlockNum || block < 0)
            {
                throw new ArgumentException($"Invalid block or module number. Actual block: {block}, Max: {BlockNum}");
            }
            APSUsStateOverrideByUser[block] = isOverridden;
        }

        public bool GetAPSUStateIsOverridden(int block)
        {
            if (block >= BlockNum || block < 0)
            {
                throw new ArgumentException($"Invalid block or module number. Actual block: {block}, Max: {BlockNum}");
            }
            return APSUsStateOverrideByUser[block];
        }

        public bool GetAPSUState(int block)
        {
            if (block >= BlockNum || block < 0)
            {
                throw new ArgumentException($"Invalid block or module number. Actual block: {block}, Max: {BlockNum}");
            }
            return APSUsState[block];
        }

        public void SetAPSUState(int block, bool enabled = false)
        {
            if (block >= BlockNum || block < 0)
            {
                throw new ArgumentException($"Invalid block or module number. Actual block: {block}, Max: {BlockNum}");
            }
            APSUsState[block] = enabled;
        }

        public CoolerSettingsModel GetCoolerSettings(int block, int module)
        {
            int index = GetIndex(block, module);
            return CoolerSettings[index];
        }

        public CoolerSettingsModel GetCopyOfCoolerSettings(int block, int module)
        {
            int index = GetIndex(block, module);
            var s = CoolerSettings[index];
            CoolerSettingsModel ns = new CoolerSettingsModel();
            ns.Enabled = s.Enabled;
            ns.EnabledByUser = s.EnabledByUser;
            ns.TargetTemperature = s.TargetTemperature;
            ns.FanSpeed = s.FanSpeed;
            ns.Block = s.Block;
            ns.Module = s.Module;
            return ns;
        }

        public void SetCoolerSettings(CoolerSettingsModel settings)
        {
            int index = GetIndex(settings.Block, settings.Module);
            CoolerSettings[index].Enabled = settings.Enabled;
            CoolerSettings[index].TargetTemperature = settings.TargetTemperature;
            CoolerSettings[index].FanSpeed = settings.FanSpeed;
            //when turned off set IsTemperatureStable to false because we don't know
            if (!settings.Enabled)
            {
                CoolerSettings[index].State.ActualState = CoolerStates.Off;
                CoolerSettings[index].State.IsTemperatureStable = false;
            }
        }

        public void SetModuleCoolerState(ModuleCoolerState state)
        {
            int index = GetIndex(state.Block, state.Module);
            if (CoolerSettings[index].State.IsTemperatureStable != state.IsTemperatureStable)
            {
                var eventArgs = new CoolerTemperatureStabilizationChangedEventArgs(CoolerSettings[index].Block, CoolerSettings[index].Module, CoolerSettings[index].State.IsTemperatureStable, state.IsTemperatureStable);
                OnCoolerTemperatureStabilizationChanged?.Invoke(this, eventArgs);
            }

            if (CoolerSettings[index].State.ActualState != state.ActualState)
            {
                if (state.ActualState != CoolerStates.Off && state.ActualState != CoolerStates.On)
                {
                    var eventArgs = new CoolerFailEventArgs(CoolerSettings[index].Block, CoolerSettings[index].Module, CoolerSettings[index].State.ActualState, state.ActualState);
                    OnCoolerFail?.Invoke(this, eventArgs);
                }
            }
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

