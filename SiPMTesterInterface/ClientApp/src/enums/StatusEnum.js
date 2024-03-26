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