export const ResponseButtons = {
	Cancel: 0,
	OK: 1,
	Stop: 2,
	Retry: 3,
	Continue: 4,
	Yes: 5,
	No: 6,
	CancelOK: 7,
	StopRetryContinue: 8,
	YesNo: 9
};

export function getResponseButtonString(respButton) {
    switch (respButton) {
        case ResponseButtons.Cancel:
            return "Cancel";
        case ResponseButtons.OK:
            return "OK";
        case ResponseButtons.Stop:
            return "Stop";
        case ResponseButtons.Retry:
            return "Retry";
        case ResponseButtons.Continue:
            return "Continue";
        case ResponseButtons.Yes:
            return "Yes";
        case ResponseButtons.No:
            return "No";
        case ResponseButtons.CancelOK:
            return "CancelOK";
        case ResponseButtons.StopRetryContinue:
            return "StopRetryContinue";
        case ResponseButtons.YesNo:
            return "YesNo";
        default:
            return "";
    }
}