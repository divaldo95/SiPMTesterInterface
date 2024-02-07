import axios from 'axios';

const API_BASE_URL = '';
const API_STATES_URL = 'measurement/states/'

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
};

export default MeasurementStateService;
