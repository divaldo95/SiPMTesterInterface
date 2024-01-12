import { useState, useEffect } from "react";
import BarcodeInput from "./BarcodeInput";
import LoadingSpinner from './LoadingSpinner';
import SelectedList from './SelectedList';
import MeasurementOrder from './MeasurementOrder';
import SiPMArray from "./SiPMArray";
import IVVoltageList from "./IVVoltageList";
import SPSVoltageList from "./SPSVoltageList";
import MeasurementStatus from "./MeasurementStatus";

function Measurement() {
    //state for steps
    const [step, setstep] = useState(1);
    const [isLoading, setIsLoading] = useState(true);
    const [nSiPMArrays, setnSiPMArrays] = useState(16); //there will be 16 arrays

    useEffect(() => {
        // Simulating an asynchronous operation
        const timeoutId = setTimeout(() => {
            setIsLoading(false);
        }, 3000);

        // Clean up the timeout when the component unmounts or when the loading is finished
        return () => clearTimeout(timeoutId);
    }, []);

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

                    {/* Switch statement for conditional content */}
                    {(() => {
                        switch (step) {
                            case 1:
                                return (
                                    <MeasurementStatus
                                        numArrays={nSiPMArrays}
                                        formData={formData}
                                    >
                                    </MeasurementStatus>
                                );
                            case 1:
                                return (
                                    <div>
                                        <SiPMArray
                                            numArrays={nSiPMArrays}
                                            formData={formData}
                                            updateSelectedSiPMs={updateSelectedSiPMs}
                                            updateIVMeas={handleUpdateIVMeas}
                                            updateSPSMeas={handleUpdateSPSMeas}
                                            nextStep={nextStep}
                                            editable={true}
                                        />
                                    </div>
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
                                    <div>
                                        <BarcodeInput nextStep={nextStep} prevStep={prevStep} formData={formData} onFormChange={handleUpdateBarcodes} nArrays={nSiPMArrays} />
                                    </div>
                                );
                            case 3:
                                return (
                                    <div>
                                        <MeasurementOrder nextStep={nextStep} prevStep={prevStep} handleFormData={handleInputData} values={formData} nArrays={nSiPMArrays} />
                                    </div>
                                );
                            case 4:
                                return (
                                    <div>
                                        <IVVoltageList
                                            nextStep={nextStep}
                                            prevStep={prevStep}
                                            updateVoltageList={handleUpdateIVVoltageList}
                                            formData={formData}
                                            updateGeneratorData={handleUpdateGeneratorData}
                                        />
                                    </div>
                                );
                            case 5:
                                return (
                                    <div>
                                        <SPSVoltageList
                                            nextStep={nextStep}
                                            prevStep={prevStep}
                                            updateVoltageList={handleUpdateSPSVoltageList}
                                            formData={formData}
                                            updateGeneratorData={handleUpdateGeneratorData}
                                        />
                                    </div>
                                );
                            case 6:
                                return (
                                    <div>
                                        <div>
                                            <SiPMArray
                                                numArrays={nSiPMArrays}
                                                formData={formData}
                                                updateSelectedSiPMs={updateSelectedSiPMs}
                                                updateIVMeas={handleUpdateIVMeas}
                                                updateSPSMeas={handleUpdateSPSMeas}
                                                nextStep={nextStep}
                                                editable={false}
                                            />
                                        </div>
                                    </div>
                                );
                            // default case to show nothing
                            default:
                                return (
                                    <div className="App">
                                    </div>
                                );
                        }
                    })()}
                </div>
            )}
        </div>
    );

    // javascript switch case to show different form in each step
    
};

export default Measurement;
