using System;
namespace SiPMTesterInterface.Enums
{
    public enum CoolerStates
    {
        On = 0,
        Off = 1,
        TemperatureSensorError = 2,
        ThermalRunaway = 3,
        BlockedFan = 4,
        PeltierError = 5
    };
}

