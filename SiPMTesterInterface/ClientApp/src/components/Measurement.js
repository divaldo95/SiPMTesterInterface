import { useState, useEffect } from "react";
import SiPMSelector from "./SiPMSelector";
import BarcodeInput from "./BarcodeInput";
import LoadingSpinner from './LoadingSpinner';
import MeasurementOrder from './MeasurementOrder';

function Measurement() {
    //state for steps
    const [step, setstep] = useState(1);
    const [isLoading, setIsLoading] = useState(true);
    const [nSiPMArrays, setnSiPMArrays] = useState(4);

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
        selectedSiPMs: Array.from({ length: nSiPMArrays }, () => Array.from({ length: 16 }, () => [true, true])),
        
    })

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
                            // case 1 to show stepOne form and passing nextStep, prevStep, and handleInputData as handleFormData method as prop and also formData as value to the fprm
                            case 1:
                                return (
                                    <div>
                                        <SiPMSelector nextStep={nextStep} handleFormData={handleInputData} values={formData} nArrays={nSiPMArrays} />
                                    </div>
                                );
                            // case 2 to show stepTwo form passing nextStep, prevStep, and handleInputData as handleFormData method as prop and also formData as value to the fprm
                            case 2:
                                return (
                                    <div>
                                        <BarcodeInput nextStep={nextStep} prevStep={prevStep} handleFormData={handleInputData} values={formData} nArrays={nSiPMArrays} />
                                    </div>
                                );
                            case 3:
                                return (
                                    <div>
                                        <MeasurementOrder nextStep={nextStep} prevStep={prevStep} handleFormData={handleInputData} values={formData} nArrays={nSiPMArrays} />
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
