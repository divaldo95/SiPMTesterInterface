// ConnectionStateEnum.js
export const ConnectionStatusEnum = {
    Disconnected: 0,
    Connected: 1,
    NotConnected: 2,
    Reconnecting: 3,
    Error: 4
};


export function getConnectionStatusBtnClasses(status) {
    switch (status) {
        case ConnectionStatusEnum.Disconnected:
            return {
                buttonColor: "btn-outline-danger",
                textColor: "",
                icon: "bi-x-circle",
            };
        case ConnectionStatusEnum.Connected:
            return {
                buttonColor: "btn-success",
                textColor: "",
                icon: "bi-check-circle",
            };
        case ConnectionStatusEnum.NotConnected:
            return {
                buttonColor: "btn-outline-warning",
                textColor: "",
                icon: "bi-x-circle",
            };
        case ConnectionStatusEnum.Reconnecting:
            return {
                buttonColor: "btn-warning",
                textColor: "",
                icon: "bi-x-circle",
            };
        case ConnectionStatusEnum.Error:
            return {
                buttonColor: "btn-danger",
                textColor: "",
                icon: "bi-x-circle-fill",
            };
        default:
            return "";
    }
}