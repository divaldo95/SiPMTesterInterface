
// MeasurementOrder.js
import React, { useState } from 'react';

const MeasurementOrder = ({ nextStep, handleFormData, prevStep, values, nArrays }) => {
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

    return (
        <form onSubmit={submitFormData}>
            <div className=" ">
                <h2>Check or adjust the measurement order</h2>
                <div className="row">
                    {values.selectedSiPMs.map((barcode, index) => (
                        <div key={index} className="form-group">
                            <label htmlFor={`barcode${index + 1}`}>{`Array ${index + 1} Barcode:`}</label>
                            <input
                                type="text"
                                id={`barcode${index + 1}`}
                                className="form-control"
                                placeholder={`Enter Barcode for Array ${index + 1}`}
                                value={barcode}
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

export default MeasurementOrder;
