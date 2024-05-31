// StatusEnum.js
export const StatusEnum = {
    NotSelected: "NotSelected",
    SelectedIV: "SelectedIV",
    SelectedSPS: "SelectedSPS",
    SelectedBoth: "SelectedBoth",
    UnderMeasurement: "UnderMeasurement",
    MeasurementFinished: "MeasurementFinished",
    MeasurementOK: "MeasurementOK",
    MeasurementFail: "MeasurementFail"
};

export const MeasurementStateEnum = {
    NotRunning: 0,
    Running: 1,
    Finished: 2,
    FinishedIV: 3,
    FinishedDMM: 4,
    FinishedSPS: 5,
    Error: 6,
    Unknown: 7
}

export const AnalysisStateEnum = {
    Unknown: 0,
    OK: 1,
    Failed: 2
}

export function MeasurementStatusString(status) {
    switch (status) {
        case MeasurementStateEnum.NotRunning:
            return "Not running";
        case MeasurementStateEnum.Running:
            return "Running";
        case MeasurementStateEnum.Finished:
            return "Finished";
        case MeasurementStateEnum.FinishedIV:
            return "Finished IV";
        case MeasurementStateEnum.FinishedDMM:
            return "Finished DMM";
        case MeasurementStateEnum.FinishedSPS:
            return "Finished SPS";
        case MeasurementStateEnum.Error:
            return "Error";
        case MeasurementStateEnum.Unknown:
            return "Unknown";
        default:
            return "";
    }
}

export function AnalysisStatusString(status) {
    switch (status) {
        case AnalysisStateEnum.Unknown:
            return "Unknown";
        case AnalysisStateEnum.OK:
            return "OK";
        case AnalysisStateEnum.Failed:
            return "Failed";
        default:
            return "";
    }
}

export function getAnalysisStatusBackgroundClass(status) {
    switch (status) {
        case AnalysisStateEnum.Unknown:
            return "";
        case AnalysisStateEnum.OK:
            return "bg-success text-light";
        case AnalysisStateEnum.Failed:
            return "bg-danger";
        default:
            return "";
    }
}

export function getStatusBackgroundClass(status) {
    switch (status) {
        case StatusEnum.NotSelected:
            return "btn-outline-dark"; // Bootstrap class for secondary background
        case StatusEnum.SelectedIV:
            return "bg-success text-light"; // Bootstrap class for primary background
        case StatusEnum.SelectedSPS:
            return "bg-primary text-light"; // Bootstrap class for success background
        case StatusEnum.SelectedBoth:
            return "bg-info"; // Bootstrap class for info background
        case StatusEnum.UnderMeasurement:
            return "bg-warning"; // Bootstrap class for warning background
        case StatusEnum.MeasurementFinished:
            return "bg-dark"; // Bootstrap class for dark background
        case StatusEnum.MeasurementOK:
            return "bg-success"; // Bootstrap class for success background
        case StatusEnum.MeasurementFail:
            return "bg-danger"; // Bootstrap class for danger background
        default:
            return "";
    }
}

export function GetStatusBorderColorClass(IV, SPS) {
    if (IV === 1 && SPS === 1) {
        return "border-info";
    }
    else if (IV === 1 && SPS === 0) {
        return "border-info";
    }
    else if (IV === 0 && SPS === 1) {
        return "border-info";
    }
    else {
        return "";
    }
}

export function getMeasurementStatusBtnClasses(status) {
    switch (status) {
        case MeasurementStateEnum.NotRunning:
            return {
                buttonColor: "btn-outline-danger",
                textColor: "",
                icon: "bi-x-circle",
            };
        case MeasurementStateEnum.Running:
            return {
                buttonColor: "btn-success",
                textColor: "",
                icon: "bi-check-circle",
            };
        case MeasurementStateEnum.Finished:
            return {
                buttonColor: "btn-success",
                textColor: "",
                icon: "bi-check-circle",
            };
        case MeasurementStateEnum.FinishedIV:
            return {
                buttonColor: "btn-success",
                textColor: "",
                icon: "bi-check-circle",
            };
        case MeasurementStateEnum.FinishedDMM:
            return {
                buttonColor: "btn-success",
                textColor: "",
                icon: "bi-check-circle",
            };
        case MeasurementStateEnum.FinishedSPS:
            return {
                buttonColor: "btn-success",
                textColor: "",
                icon: "bi-check-circle",
            };
        case MeasurementStateEnum.Error:
            return {
                buttonColor: "btn-danger",
                textColor: "",
                icon: "bi-x-circle-fill",
            };
        case MeasurementStateEnum.Unknown:
            return {
                buttonColor: "btn-outline-secondary",
                textColor: "",
                icon: "bi-dash-circle",
            };
        default:
            return "";
    }
}

export function GetSelectedColorClass(IV, SPS) {
    if (IV === 1 && SPS === 1) {
        return getStatusBackgroundClass(StatusEnum.SelectedBoth);
    }
    else if (IV === 1 && SPS === 0) {
        return getStatusBackgroundClass(StatusEnum.SelectedIV);
    }
    else if (IV === 0 && SPS === 1) {
        return getStatusBackgroundClass(StatusEnum.SelectedSPS);
    }
    else {
        return getStatusBackgroundClass(StatusEnum.NotSelected);
    }
}

//quick test, add all functionality later
export function GetMeasurementStateColorClass(IV) {
    if (IV === true) {
        return getStatusBackgroundClass(StatusEnum.SelectedBoth);
    }
    else {
        return getStatusBackgroundClass(StatusEnum.NotSelected);
    }
}