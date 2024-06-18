import axios from 'axios';

const API_BASE_URL = 'measurement/';
const API_TIMES_URL = 'times/'
const API_STATES_URL = 'states/'
const API_START_URL = 'start/'
const API_STOP_URL = 'stop/'
const API_DATA_URL = 'getsipmdata/'
const API_SIPM_MEAS_STATUS_URL = 'measurementstates/'

const API_PULSER_STATE_URL = 'pulser/'
const API_PULSER_DETAIL_URL = API_PULSER_STATE_URL + 'details/'
const API_COOLER_URL = 'cooler/'

const API_ALL_LOGS = 'logs/all/'
const API_UNRESOLVED_LOGS = 'logs/unresolved/'
const API_NEEDSATTENTION_LOGS = 'logs/needsattention/'
const API_RESOLVE_LOGS = 'logs/resolve/'

const MeasurementStateService = {
    getMeasurementStates: async () => {
        try {
            const response = await axios.get(API_BASE_URL + API_STATES_URL);
            return response.data;
        } catch (error) {
            console.error('Error fetching measurement states:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    startMeasurement: async (jsonData) => {
        try {
            const json = JSON.stringify(jsonData);
            const response = await axios.post(API_BASE_URL + API_START_URL, json, {
                headers: {
                    // Overwrite Axios's automatically set Content-Type
                    'Content-Type': 'application/json'
                }
            });
            return response.data;
        } catch (error) {
            console.error('Error starting measurement:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    stopMeasurement: async () => {
        try {
            const response = await axios.post(API_BASE_URL + API_STOP_URL);
            return response.data;
        } catch (error) {
            console.error('Error stopping measurement:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    getMeasurementData: async () => {
        try {
            const response = await axios.get(API_BASE_URL + API_DATA_URL);
            return response.data;
        } catch (error) {
            console.error('Error fetching measurement data:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    getSiPMMeasurementData: async (blockIndex, moduleIndex, arrayIndex, sipmIndex) => {
        try {
            const response = await axios.get(API_BASE_URL + API_DATA_URL + blockIndex + "/" + moduleIndex + "/" + arrayIndex + "/" + sipmIndex + "/");
            return response.data;
        } catch (error) {
            console.error('Error fetching sipm measurement data:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    getMeasurementTimes: async () => {
        try {
            const response = await axios.get(API_BASE_URL + API_TIMES_URL);
            return response.data;
        } catch (error) {
            console.error('Error fetching sipm time data:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    getMeasuredSiPMStates: async () => {
        try {
            const response = await axios.get(API_BASE_URL + API_SIPM_MEAS_STATUS_URL);
            return response.data;
        } catch (error) {
            console.error('Error fetching sipm measurements statuses:', error);
            throw error; // You can handle the error as needed in your application
        }
    },

    getPulserState: async () => {
        try {
            const response = await axios.get(API_BASE_URL + API_PULSER_STATE_URL);
            return response.data;
        } catch (error) {
            console.error('Error fetching pulser state:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    getPulserStateDetails: async () => {
        try {
            const response = await axios.get(API_BASE_URL + API_PULSER_DETAIL_URL);
            return response.data;
        } catch (error) {
            console.error('Error fetching pulser state details:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    getAllLogs: async () => {
        try {
            const response = await axios.get(API_BASE_URL + API_ALL_LOGS);
            return response.data;
        } catch (error) {
            console.error('Error fetching all logs:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    getUnresolvedLogs: async () => {
        try {
            const response = await axios.get(API_BASE_URL + API_UNRESOLVED_LOGS);
            return response.data;
        } catch (error) {
            console.error('Error fetching unresolved logs:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    getNeedsAttentionLogs: async () => {
        try {
            const response = await axios.get(API_BASE_URL + API_NEEDSATTENTION_LOGS);
            return response.data;
        } catch (error) {
            console.error('Error fetching errors which needs attention:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    setLogResponse: async (id, responseButton) => {
        try {
            const data = {
                ID: id,
                UserResponse: responseButton
            };
            //console.log(data);
            const json = JSON.stringify(data);
            const response = await axios.post(API_BASE_URL + API_RESOLVE_LOGS, json, {
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            return response.data;
        } catch (error) {
            if (error.response) {
                if (error.response.status === 400) {
                    console.error('BadRequest:', error.response.data);
                    throw new Error('Bad request: ' + (error.response.data.message || 'Invalid request.'));
                } else {
                    console.error('Error response:', error.response);
                    throw new Error('An error occurred: ' + error.response.status);
                }
            } else if (error.request) {
                console.error('No response received:', error.request);
                throw new Error('No response received from the server.');
            } else {
                console.error('Error setting up the request:', error.message);
                throw new Error('Error setting up the request: ' + error.message);
            }
        }
    },
    setPulser: async (secInterval) => {
        try {
            const data = {
                RefreshInterval: secInterval
            };
            const json = JSON.stringify(data);
            const response = await axios.post(API_BASE_URL + API_PULSER_STATE_URL, json, {
                headers: {
                    // Overwrite Axios's automatically set Content-Type
                    'Content-Type': 'application/json'
                }
            });
            return response.data;
        } catch (error) {
            console.error('Error setting pulser state:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    setCooler: async (block, module, enabled, targetTemperature, fanSpeed) => {
        try {
            const data = {
                Block: block,
                Module: module,
                Enabled: enabled,
                TargetTemperature: targetTemperature,
                FanSpeed: fanSpeed,
            };
            const json = JSON.stringify(data);
            const response = await axios.post(API_BASE_URL + API_COOLER_URL, json, {
                headers: {
                    // Overwrite Axios's automatically set Content-Type
                    'Content-Type': 'application/json'
                }
            });
            return response.data;
        } catch (error) {
            console.error('Error setting cooler state:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
    getRootFile: async (blockIndex, moduleIndex, arrayIndex, sipmIndex) => {
        try {
            const response = await axios.get(API_BASE_URL + API_DATA_URL + blockIndex + "/" + moduleIndex + "/" + arrayIndex + "/" + sipmIndex + "/RootFile", {
                responseType: 'blob'
            });
            return response.data;
        } catch (error) {
            console.error('Error fetching sipm measurement data file:', error);
            throw error; // You can handle the error as needed in your application
        }
    },
};

export default MeasurementStateService;
