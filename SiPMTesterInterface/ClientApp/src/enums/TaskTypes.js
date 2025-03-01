﻿export const TaskTypes = {
    DMMResistance: 0,
    IV: 1,
    SPS: 2,
    SPSDMM: 3,
    Waiting: 4,
    Finished: 5,
    Idle: 6,
    TemperatureStabilisation: 7,
    DarkCurrent: 8,
    ForwardResistance: 9,
    BlockDisable: 10,
    BlockEnable: 11,
    TemperatureMeasurement: 12,
    PulserChange: 13,
    Analysis: 14
}  

export function TaskTypesString(type) {
    switch (type) {
        case TaskTypes.DMMResistance:
            return "DMM";
        case TaskTypes.IV:
            return "IV";
        case TaskTypes.SPS:
            return "SPS";
        case TaskTypes.SPSDMM:
            return "SPS V";
        case TaskTypes.Waiting:
            return "Waiting";
        case TaskTypes.Finished:
            return "Finished";
        case TaskTypes.Idle:
            return "Idle";
        case TaskTypes.TemperatureStabilisation:
            return "Waiting for Temperature";
        case TaskTypes.DarkCurrent:
            return "Dark Current";
        case TaskTypes.ForwardResistance:
            return "Forward Resistance";
        case TaskTypes.BlockDisable:
            return "Block Disable";
        case TaskTypes.BlockEnable:
            return "Block Enable";
        case TaskTypes.TemperatureMeasurement:
            return "Temperature";
        case TaskTypes.PulserChange:
            return "Pulser Change";
        case TaskTypes.Analysis:
            return "Analysis";
        default:
            return "";
    }
}