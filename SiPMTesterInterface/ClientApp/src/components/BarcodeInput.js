// BarcodeInput.js
import React, { useState } from 'react';
import './BarcodeInput.css';

const BarcodeInput = ({ nextStep, prevStep, formData, onFormChange, nArrays }) => {
    const [error, setError] = useState(false);

    const submitFormData = (e) => {
        e.preventDefault();

        // checking if value of first name and last name is empty show error else take to next step
        //if (validator.isEmpty(values.age) || validator.isEmpty(values.email)) {
        //    setError(true);
        //} else {
            nextStep();
        //}
    };

    const handleBarcodeChange = (arrayIndex, e) => {
        const newBarcodes = [...formData.barcodes];
        newBarcodes[arrayIndex] = e.target.value;

        // Invoke the callback to notify the upper-level component about the change
        onFormChange(newBarcodes);
    };

    const isAnySiPMSelected = (arrayIndex) => {
        return formData.selectedSiPMs[arrayIndex].some(
            (sipm) => sipm
        );
    };

    const isContinueButtonDisabled = () => {
        return formData.selectedSiPMs.some((sipms, arrayIndex) => {
            return (
                sipms.some((sipm) => sipm) &&
                (!formData.barcodes[arrayIndex] || !formData.barcodes[arrayIndex].trim())
            );
        });
    };

    const isBarcodeValid = (arrayIndex) => {
        const barcode = formData.barcodes[arrayIndex];
        return (/\S+/.test(barcode) || !isAnySiPMSelected(arrayIndex));
    };

    /*
    //Module Tester things
     const isBarcodeInvalid = formData.selectedSiPMs.some(
        (array, arrayIndex) => array.some((sipm) => sipm && (sipm[0] || sipm[1])) && !formData.barcodes[arrayIndex].trim()
    );

    const isAnySiPMSelected = (arrayIndex) => {
        return formData.selectedSiPMs[arrayIndex].some(
            (sipm) => sipm && (sipm[0] || sipm[1])
        );
    };

    const isContinueButtonDisabled = () => {
        return formData.selectedSiPMs.some((sipms, arrayIndex) => {
            return (
                sipms.some((sipm) => sipm && (sipm[0] || sipm[1])) &&
                (!formData.barcodes[arrayIndex] || !formData.barcodes[arrayIndex].trim())
            );
        });
    };
     */

    return (
        <form onSubmit={submitFormData} noValidate>
            <div className=" ">
                <div>
                    <div className="row justify-content-center">
                        <div className="col-md-8">
                            {formData.barcodes.map((barcode, arrayIndex) => (
                                <div className="col mb-4">
                                    <div className="d-flex flex-column h-100">
                                        <div key={arrayIndex} className="card flex-grow-1">
                                            <h4 className="card-header d-flex justify-content-between align-items-center">
                                                Array {arrayIndex} barcode:
                                            </h4>
                                            <div className="card-body d-flex flex-wrap">
                                                <input
                                                    type="text"
                                                    id={`barcodeInput${arrayIndex}`}
                                                    className={`form-control ${!isBarcodeValid(arrayIndex) ? 'is-invalid' : ''}`}
                                                    placeholder={`Enter Barcode for Array ${arrayIndex + 1}`}
                                                    value={barcode}
                                                    onChange={(e) => handleBarcodeChange(arrayIndex, e)}
                                                    required // HTML5 form validation
                                                    pattern="\S+" // Ensures non-whitespace characters are entered
                                                    disabled={!isAnySiPMSelected(arrayIndex)} // Disable input if no SiPM is selected in the array
                                                />
                                                <div className="invalid-feedback">Barcode is required if at least one SiPM is selected.</div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                    <div class="d-grid gap-4 col-6 mx-auto mb-4">
                        <div className="clearfix">
                            <button
                            className="btn btn-secondary float-start"
                                onClick={prevStep}>
                                Back
                            </button>
                            <button
                                className={`btn float-end ${!isContinueButtonDisabled() ? 'btn-success' : 'btn-danger'}`}
                                type="submit"
                                disabled={isContinueButtonDisabled()}
                            >
                                Continue
                            </button>
                        </div>
                    </div>
                </div>
            </div>
            
        </form>
    );
};

export default BarcodeInput;
