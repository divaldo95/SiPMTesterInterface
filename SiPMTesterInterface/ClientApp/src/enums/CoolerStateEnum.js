export const CoolerState = {
    On: 0,
    Off: 1,
    TemperatureSensorError: 2,
    ThermalRunaway: 3,
    BlockedFan: 4
}

export function CoolerStateProps(status) {
    switch (status) {
        case CoolerState.On:
            return {
                StateMessage: "On",
                Color: "success",
            }
        case CoolerState.Off:
            return {
                StateMessage: "Off",
                Color: "secondary",
            }
        case CoolerState.TemperatureSensorError:
            return {
                StateMessage: "Temperature sensor error",
                Color: "danger",
            }
        case CoolerState.ThermalRunaway:
            return {
                StateMessage: "Thermal runaway",
                Color: "danger",
            }
        case CoolerState.BlockedFan:
            return {
                StateMessage: "Blocked fan",
                Color: "danger",
            }
        default:
            return "";
    }
}