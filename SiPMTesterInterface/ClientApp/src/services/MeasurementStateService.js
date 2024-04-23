import axios from 'axios';

const API_BASE_URL = 'measurement/';
const API_TIMES_URL = 'times/'
const API_STATES_URL = 'states/'
const API_START_URL = 'start/'
const API_DATA_URL = 'getsipmdata/'
const API_SIPM_MEAS_STATUS_URL = 'measurementstates/'

const API_PULSER_STATE_URL = 'pulser/'
const API_PULSER_DETAIL_URL = API_PULSER_STATE_URL + 'details/'
const API_COOLER_URL = 'cooler/'

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
            console.error('Error fetching measurement states:', error);
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
};

export default MeasurementStateService;
