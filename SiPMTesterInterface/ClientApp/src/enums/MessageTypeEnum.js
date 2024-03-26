// MessageTypeEnum.js
export const MessageTypeEnum = {
    Information: "Information",
    Warning: "Warning",
    Error: "Error",
    Debug: "Debug"
};

export function getIconClass(type) {
    switch (type) {
        case MessageTypeEnum.Information:
            return "bi-info-circle"; 
        case MessageTypeEnum.Warning:
            return "bi-exclamation-triangle";
        case MessageTypeEnum.Error:
            return "bi-exclamation-circle";
        case MessageTypeEnum.Debug:
            return "bi-wrench-adjustable-circle";
        default:
            return "";
    }
}