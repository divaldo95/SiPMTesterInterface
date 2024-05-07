export const SerialConnectionStateEnum = {
    Disabled: 0,
    Disconnected: 1,
    Connected: 2,
    Timeout: 3
}

export function ConnectionStatusString(status) {
    switch (status) {
        case SerialConnectionStateEnum.Disabled:
            return "Disabled";
        case SerialConnectionStateEnum.Connected:
            return "Connected";
        case SerialConnectionStateEnum.Disconnected:
            return "Disconnected";
        case SerialConnectionStateEnum.Timeout:
            return "Timeout";
        default:
            return "";
    }
}

export function getConnectionStatusBtnClasses(status) {
    switch (status) {
        case SerialConnectionStateEnum.Disabled:
            return {
                buttonColor: "btn-outline-secondary",
                textColor: "",
                icon: "bi-dash-circle",
            };
        case SerialConnectionStateEnum.Disconnected:
            return {
                buttonColor: "btn-outline-danger",
                textColor: "text-white",
                icon: "bi-x-circle",
            };
        case SerialConnectionStateEnum.Connected:
            return {
                buttonColor: "btn-success",
                textColor: "text-white",
                icon: "bi-check-circle",
            };
        case SerialConnectionStateEnum.Timeout:
            return {
                buttonColor: "btn-warning",
                textColor: "",
                icon: "bi-check-circle",
            };
        default:
            return {
                buttonColor: "",
                textColor: "",
                icon: "",
            };
    }
}