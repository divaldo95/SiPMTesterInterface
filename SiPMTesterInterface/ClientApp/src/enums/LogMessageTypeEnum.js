export const LogMessageType = {
	Info: 0,
	Warning: 1,
	Error: 2,
	Fatal: 3,
	Debug: 4
}

export function getLogMessageTypeMarkDetails(type) {
    switch (type) {
        case LogMessageType.Info:
            return {
                Mark: "bi-info-circle-fill",
                Color: "text-dark",
                Tooltip: "Info"
            }
        case LogMessageType.Warning:
            return {
                Mark: "bi-exclamation-triangle-fill",
                Color: "text-warning",
                Tooltip: "Warning"
            }
        case LogMessageType.Error:
            return {
                Mark: "bi-exclamation-circle-fill",
                Color: "text-danger",
                Tooltip: "Error"
            }
        case LogMessageType.Fatal:
            return {
                Mark: "bi-exclamation-octagon-fill",
                Color: "text-danger",
                Tooltip: "Fatal"
            }
        case LogMessageType.Debug:
            return {
                Mark: "bi-bug-fill",
                Color: "text-primary",
                Tooltip: "Debug"
            }
        default:
            return {
                Mark: "bi-question-circle-fill",
                Color: "text-dark",
                Tooltip: "Unknown log type"
            }
    }
}