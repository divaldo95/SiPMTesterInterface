export const TaskTypes = {
    DMMResistance: 0,
    IV: 1,
    SPS: 2,
    SPSDMM: 3,
    Waiting: 4,
    Finished: 5,
    Idle: 6
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
        default:
            return "";
    }
}