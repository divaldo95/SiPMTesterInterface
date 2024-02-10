﻿import { useState, useEffect } from "react";
import BarcodeInput from "./BarcodeInput";
import LoadingSpinner from './LoadingSpinner';
import SelectedList from './SelectedList';
import MeasurementOrder from './MeasurementOrder';
import SiPMArray from "./SiPMArray";
import IVVoltageList from "./IVVoltageList";
import SPSVoltageList from "./SPSVoltageList";
import MeasurementStatus from "./MeasurementStatus";
import MeasurementStateService from '../services/MeasurementStateService';

import { HubConnectionBuilder } from '@microsoft/signalr';

function Measurement() {
    //state for steps
    const [step, setstep] = useState(1);
    const [isLoading, setIsLoading] = useState(true);
    const [nSiPMArrays, setnSiPMArrays] = useState(16); //there will be 16 arrays
    const [measurementStates, setMeasurementStates] = useState(null);
    const [measurementRunning, setMeasurementRunning] = useState(false);

    const [testState, setTestState] = useState("TEST");

    useEffect(() => {
        const connection = new HubConnectionBuilder()
            .withUrl('hub')
            .withAutomaticReconnect()
            .build();
        connection.start()
            .then(result => {
                setTestState("Connected");

                connection.on('ReceiveGlobalStateChange', (globalStateModel) => {
                    // Handle received global state change
                    
                    console.log('Received global state change:', globalStateModel);
                });

                connection.on('ReceiveIVMeasurementStateChange', (ivModel) => {
                    // Handle received IV measurement state change
                    setTestState(ivModel);
                    console.log('Received IV measurement state change:', ivModel);
                });

                connection.on('ReceiveSPSMeasurementStateChange', (spsModel) => {
                    // Handle received SPS measurement state change
                    console.log('Received SPS measurement state change:', spsModel);
                });
            })

            .catch(e => {
                console.log('Connection failed: ', e);
                setTestState("Connection failed");
            });

        const fetchData = async () => {
            try {
                const data = await MeasurementStateService.getMeasurementStates();
                setMeasurementStates(data);
                setIsLoading(false);
                //alert(data.spsState);
                if (data.ivState === 0 && data.spsState === 0) {
                    setMeasurementRunning(false);
                }
                else {
                    setMeasurementRunning(true);
                }
            } catch (error) {
                // Handle the error if needed
            }
        };

        fetchData();
    }, []); // Empty dependency array ensures the effect runs only once when the component mounts

    /*
    useEffect(() => {
        // Simulating an asynchronous operation
        const timeoutId = setTimeout(() => {
            setIsLoading(false);
        }, 3000);

        // Clean up the timeout when the component unmounts or when the loading is finished
        return () => clearTimeout(timeoutId);
    }, []);
    */

    //state for form data
    const [formData, setFormData] = useState({
        //selectedSiPMs: Array.from({ length: nSiPMArrays }, () => Array.from({ length: 16 }, () => [true, true])),
        selectedSiPMs: Array.from({ length: nSiPMArrays }, () => Array.from({ length: 16 }, () => true)),
        barcodes: Array.from({ length: nSiPMArrays }, () => ""),
        voltageListIV: [], // Add a state for the voltage list
        voltageListSPSOffset: [], // Add a state for the voltage list
        startIVValue: 35.0,
        endIVValue: 42.0,
        stepIVValue: 0.2,
        offsetSPS: 1.0,
        stepSPS: 0.5,
        countSPS: 3,
        ivMeasurementEnabled: true,
        spsMeasurementEnabled: true
    });

    const [selectedList, setSelectedList] = useState([]);

    const handleFormChange = (newFormData) => {
        // Update the formData state in the upper-level component
        setFormData(newFormData);

        // Update the selectedList when the form data changes
        const newSelectedList = formData.barcodes.map((barcode, arrayIndex) => ({
            barcode,
            selectedSiPMs: formData.selectedSiPMs[arrayIndex],
        }));

        setSelectedList(newSelectedList);
    };

    const handleReorder = (sourceIndex, destinationIndex) => {
        // Reorder the selectedSiPMs array based on drag-and-drop
        const newSelectedSiPMs = [...formData.selectedSiPMs];
        const [movedArray] = newSelectedSiPMs.splice(sourceIndex, 1);
        newSelectedSiPMs.splice(destinationIndex, 0, movedArray);

        setFormData({
            ...formData,
            selectedSiPMs: newSelectedSiPMs,
        });

        // Update the selectedList
        const newSelectedList = formData.barcodes.map((barcode, arrayIndex) => ({
            barcode,
            selectedSiPMs: newSelectedSiPMs[arrayIndex],
        }));

        setSelectedList(newSelectedList);
    };

    // Function to update selectedSiPMs in formData
    const updateSelectedSiPMs = (newSelectedSiPMs) => {
        setFormData((prevFormData) => ({
            ...prevFormData,
            selectedSiPMs: newSelectedSiPMs
        }));
    };

    // Function to update Voltage List in formData
    const handleUpdateIVVoltageList = (updatedVoltageList) => {
        setFormData((prevFormData) => ({
            ...prevFormData,
            voltageListIV: updatedVoltageList,
        }));
    };

    // Function to update Voltage List in formData
    const handleUpdateSPSVoltageList = (updatedVoltageList) => {
        setFormData((prevFormData) => ({
            ...prevFormData,
            voltageListSPSOffset: updatedVoltageList,
        }));
    };

    // Function to update Barcodes in formData
    const handleUpdateBarcodes = (updatedBarcodes) => {
        setFormData((prevFormData) => ({
            ...prevFormData,
            barcodes: updatedBarcodes,
        }));
    };

    // Function to update Voltage Generator data in formData
    const handleUpdateGeneratorData = (start, end, step) => {
        setFormData((prevFormData) => ({
            ...prevFormData,
            startValue: start,
            endValue: end,
            stepValue: step,
        }));
    };

    const handleUpdateIVMeas = (enabled) => {
        setFormData((prevFormData) => ({
            ...prevFormData,
            ivMeasurementEnabled: enabled,
        }));
    };

    const handleUpdateSPSMeas = (enabled) => {
        setFormData((prevFormData) => ({
            ...prevFormData,
            spsMeasurementEnabled: enabled,
        }));
    };

    // function for going to next step by increasing step state by 1
    const nextStep = () => {
        setstep(step + 1);
    };

    // function for going to previous step by decreasing step state by 1
    const prevStep = () => {
        setstep(step - 1);
    };

    // handling form input data by taking onchange value and updating our previous form data state
    const handleInputData = input => e => {
        console.log(input);
        
        // input value from the form
        const { value } = e.target;
        alert(value);

        //updating for data state taking previous state and then adding new value to create new object
        setFormData(prevState => ({
            ...prevState,
            [input]: value
        }));
    }

    return (
        <div>
            {isLoading ? (
                <LoadingSpinner />
            ) : (
                <div>
                    {(() => {
                        if (measurementRunning) {
                            return (
                                <MeasurementStatus
                                    numArrays={nSiPMArrays}
                                    formData={formData}
                                    measurementStates={measurementStates}
                                />
                            );
                        } else {
                            switch (step) {
                                case 1:
                                    return (
                                        <SiPMArray
                                            numArrays={nSiPMArrays}
                                            formData={formData}
                                            updateSelectedSiPMs={updateSelectedSiPMs}
                                            updateIVMeas={handleUpdateIVMeas}
                                            updateSPSMeas={handleUpdateSPSMeas}
                                            nextStep={nextStep}
                                            editable={true}
                                        />
                                    );
                                // case 1 to show stepOne form and passing nextStep, prevStep, and handleInputData as handleFormData method as prop and also formData as value to the fprm
                                // case 6:
                                //    return (
                                //        <div>
                                //            <SiPMSelector nextStep={nextStep} formData={formData} onFormChange={handleFormChange} nArrays={nSiPMArrays} />
                                //        </div>
                                //    );
                                // case 2 to show stepTwo form passing nextStep, prevStep, and handleInputData as handleFormData method as prop and also formData as value to the fprm
                                case 2:
                                    return (
                                        <BarcodeInput nextStep={nextStep} prevStep={prevStep} formData={formData} onFormChange={handleUpdateBarcodes} nArrays={nSiPMArrays} />
                                    );
                                case 3:
                                    return (
                                        <MeasurementOrder nextStep={nextStep} prevStep={prevStep} handleFormData={handleInputData} values={formData} nArrays={nSiPMArrays} />
                                    );
                                case 4:
                                    return (
                                        <IVVoltageList
                                            nextStep={nextStep}
                                            prevStep={prevStep}
                                            updateVoltageList={handleUpdateIVVoltageList}
                                            formData={formData}
                                            updateGeneratorData={handleUpdateGeneratorData}
                                        />
                                    );
                                case 5:
                                    return (
                                        <SPSVoltageList
                                            nextStep={nextStep}
                                            prevStep={prevStep}
                                            updateVoltageList={handleUpdateSPSVoltageList}
                                            formData={formData}
                                            updateGeneratorData={handleUpdateGeneratorData}
                                        />
                                    );
                                case 6:
                                    return (
                                        <SiPMArray
                                            numArrays={nSiPMArrays}
                                            formData={formData}
                                            updateSelectedSiPMs={updateSelectedSiPMs}
                                            updateIVMeas={handleUpdateIVMeas}
                                            updateSPSMeas={handleUpdateSPSMeas}
                                            nextStep={nextStep}
                                            editable={false}
                                        />
                                    );
                                // default case to show nothing
                                default:
                                    return (
                                        <div className="App">
                                        </div>
                                    );
                                }
                            }
                        })()}
                </div>
            )}
        </div>
    );

    // javascript switch case to show different form in each step
    
};

export default Measurement;
