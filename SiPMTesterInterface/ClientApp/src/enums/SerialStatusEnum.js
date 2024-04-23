export const SerialConnectionStateEnum = {
    Connected: true,
    Disconnected: false
}

export function ConnectionStatusString(status) {
    switch (status) {
        case SerialConnectionStateEnum.Connected:
            return "Connected";
        case SerialConnectionStateEnum.Disconnected:
            return "Disconnected";
        default:
            return "";
    }
}

export function getConnectionStatusBtnClasses(status) {
    switch (status) {
        case SerialConnectionStateEnum.Disconnected:
            return {
                buttonColor: "btn-outline-danger",
                textColor: "",
                icon: "bi-x-circle",
            };
        case SerialConnectionStateEnum.Connected:
            return {
                buttonColor: "btn-success",
                textColor: "",
                icon: "bi-check-circle",
            };
        default:
            return "";
    }
}