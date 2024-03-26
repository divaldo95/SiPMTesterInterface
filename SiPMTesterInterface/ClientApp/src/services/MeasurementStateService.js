import axios from 'axios';

const API_BASE_URL = 'measurement/';
const API_TIMES_URL = 'times/'
const API_STATES_URL = 'states/'
const API_START_URL = 'start/'
const API_DATA_URL = 'getsipmdata/'

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
};

export default MeasurementStateService;
