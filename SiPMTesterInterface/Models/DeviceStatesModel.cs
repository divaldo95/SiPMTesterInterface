using System;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Models
{
    public class DeviceStatesModel
    {
        private DeviceStates[] states;

        public void SetEnabled(Devices dev, bool state)
        {
            if (state)
            {
                states[(int)dev] = DeviceStates.Enabled;
            }
            else
            {
                states[(int)dev] = DeviceStates.Disabled;

            }
        }

        public void SetInitState(Devices dev, bool state)
        {
            /*
			if (states[(int)dev] != DeviceStates.Enabled && state)
			{
				throw new ArgumentException($"Cannot set state to initialised while device ({dev}) state is {states[(int)dev]}");
			}
			else if (states[(int)dev] != DeviceStates.Initialized && !state)
			{
                throw new ArgumentException($"Cannot unset initialised state while device ({dev}) state is {states[(int)dev]}");
            }
            */

            if (state)
            {
                states[(int)dev] = DeviceStates.Enabled;
            }
            else
            {
                states[(int)dev] = DeviceStates.Disabled;

            }
        }

        public void SetStartedState(Devices dev, bool state)
        {
            /*
            if (states[(int)dev] != DeviceStates.Initialized && state)
            {
                throw new ArgumentException($"Cannot set state to started while device ({dev}) state is {states[(int)dev]}");
            }
            else if (states[(int)dev] != DeviceStates.Started && !state)
            {
                throw new ArgumentException($"Cannot unset started state while device ({dev}) state is {states[(int)dev]}");
            }
            */

            if (state)
            {
                states[(int)dev] = DeviceStates.Started;
            }
            else
            {
                states[(int)dev] = DeviceStates.Initialized;

            }
        }

        public bool IsAllEnabledDeviceStarted()
        {
            bool isOK = true;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i] != DeviceStates.Disabled && states[i] != DeviceStates.Started && states[i] != DeviceStates.Unknown)
                {
                    isOK = false;
                    break;
                }
            }
            return isOK;
        }

        public DeviceStatesModel()
		{
			states = new DeviceStates[Enum.GetNames(typeof(DeviceStates)).Length];
		}
	}
}

