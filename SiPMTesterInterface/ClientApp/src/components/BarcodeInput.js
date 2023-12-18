// BarcodeInput.js
import React, { useState } from 'react';
import './BarcodeInput.css';

const BarcodeInput = ({ nextStep, handleFormData, prevStep, values, nArrays }) => {
    const [barcodes, setBarcodes] = useState(Array.from({ length: nArrays }, () => '')); // You can adjust the number based on your requirement
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

    const handleBarcodeChange = (index, value) => {
        const newBarcodes = [...barcodes];
        newBarcodes[index] = value;
        setBarcodes(newBarcodes);
    };

    const handleContinue = () => {
        // Handle the continue action, e.g., submit the barcodes to the server
        console.log('Barcodes:', barcodes);
    };

    return (
        <form onSubmit={submitFormData}>
            <div className=" ">
                <h2>Enter Barcodes for Each Array</h2>
                <div className="row">
                    {barcodes.map((barcode, index) => (
                        <div key={index} className="form-group">
                            <label htmlFor={`barcode${index + 1}`}>{`Array ${index + 1} Barcode:`}</label>
                            <input
                                type="text"
                                id={`barcode${index + 1}`}
                                className="form-control"
                                placeholder={`Enter Barcode for Array ${index + 1}`}
                                value={barcode}
                                onChange={(e) => handleBarcodeChange(index, e.target.value)}
                            />
                        </div>
                    ))}
                </div>
                    <div class="d-grid gap-4 col-6 mx-auto">
                    <div className="clearfix">
                            <button
                            className="btn btn-secondary float-start"
                                onClick={prevStep}>
                                Back
                            </button>
                            <button
                            className="btn float-end btn-success"
                                type="submit"
                            >
                                Continue
                            </button>
                        </div>
                    </div>
                </div>
            
        </form>
    );
};

export default BarcodeInput;
