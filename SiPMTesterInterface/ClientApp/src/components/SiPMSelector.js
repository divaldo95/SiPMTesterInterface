// SiPMSelector.js
import React, { useState } from 'react';
import './SiPMSelector.css';
import BarcodeInput from './BarcodeInput';

const SiPMSelector = ({ nextStep, formData, onFormChange, nArrays }) => {

    const isAnySiPMSelected = formData.selectedSiPMs.some((array) => array.some((sipm) => sipm && (sipm[0] || sipm[1])));
    const [error, setError] = useState(false);

    const handleSiPMClick = (arrayIndex, sipmIndex, boxIndex, event) =>  {
        const newSelectedSiPMs = [...formData.selectedSiPMs];
        newSelectedSiPMs[arrayIndex] = newSelectedSiPMs[arrayIndex] || [];
        newSelectedSiPMs[arrayIndex][sipmIndex] = newSelectedSiPMs[arrayIndex][sipmIndex] || [];

        // Toggle the selected state of the clicked box
        newSelectedSiPMs[arrayIndex][sipmIndex][boxIndex] = !newSelectedSiPMs[arrayIndex][sipmIndex][boxIndex];

        //setSelectedSiPMs(newSelectedSiPMs);
        // Invoke the callback to notify the upper-level component about the change
        onFormChange({
            ...formData,
            selectedSiPMs: newSelectedSiPMs,
        });
    };

    // after form submit validating the form data using validator
    const submitFormData = (e) => {
        e.preventDefault();

        // checking if value of first name and last name is empty show error else take to step 2
        if ( !isAnySiPMSelected ) {
            setError(true);
        } else {
            nextStep();
        }
    };

    /*
    const handleSelectArrayAll = (arrayIndex) => (event) => {
        event.preventDefault();
        // Toggle select all state for the specific array
        const newSelectedSiPMs = [...selectedSiPMs];
        newSelectedSiPMs[arrayIndex] = newSelectedSiPMs[arrayIndex] || [];
        const selectAllState = newSelectedSiPMs[arrayIndex].every((sipm) => sipm && sipm[0] && sipm[1]);

        newSelectedSiPMs[arrayIndex] = Array.from({ length: 16 }, () => [!selectAllState, !selectAllState]);
        setSelectedSiPMs(newSelectedSiPMs);
    };
    */

    const handleSelectArrayAll = (arrayIndex) => (e) => {
        e.preventDefault();
        // Toggle select all state for the specific array
        onFormChange((prevFormData) => {
            e.preventDefault();
            // Toggle select all state for the specific array
            const newSelectedSiPMs = [...formData.selectedSiPMs];
            newSelectedSiPMs[arrayIndex] = newSelectedSiPMs[arrayIndex] || [];
            const selectAllState = newSelectedSiPMs[arrayIndex].every((sipm) => sipm && sipm[0] && sipm[1]);

            newSelectedSiPMs[arrayIndex] = Array.from({ length: 16 }, () => [!selectAllState, !selectAllState]);

            return {
                ...formData,
                selectedSiPMs: newSelectedSiPMs,
            };
        });
    };

    const handleSelectAll = (e) => {
        e.preventDefault();
        // Toggle select all state for the specific array
        onFormChange((prevFormData) => {
            e.preventDefault();
            // Toggle select all state for all arrays
            const selectAllState = formData.selectedSiPMs.flat().every((sipm) => sipm && sipm[0] && sipm[1]);

            const newSelectedSiPMs = Array.from({ length: nArrays }, () =>
                Array.from({ length: 16 }, () => [!selectAllState, !selectAllState])
            );

            return {
                ...formData,
                selectedSiPMs: newSelectedSiPMs,
            };
        });
    };

    /*
    const handleSelectAll = (e) => {
        e.preventDefault();
        // Toggle select all state for all arrays
        const selectAllState = selectedSiPMs.flat().every((sipm) => sipm && sipm[0] && sipm[1]);

        const newSelectedSiPMs = Array.from({ length: nArrays }, () =>
            Array.from({ length: 16 }, () => [!selectAllState, !selectAllState])
        );

        setSelectedSiPMs(newSelectedSiPMs);
        onFormChange({
            ...formData,
            selectedSiPMs: newSelectedSiPMs,
        });
    };
    */

    const renderArrays = () => {
        const arrays = [];

        for (let arrayIndex = 0; arrayIndex < nArrays; arrayIndex++) {
            const sipmArray = [];

            for (let sipmIndex = 0; sipmIndex < 16; sipmIndex++) {
                const isSelected = formData.selectedSiPMs[arrayIndex] && formData.selectedSiPMs[arrayIndex][sipmIndex];

                sipmArray.push(
                    <div key={sipmIndex} className="sipm-container">
                        <div
                            className={`sipm ${isSelected && formData.selectedSiPMs[arrayIndex][sipmIndex][0] ? 'selected' : ''}`}
                            onClick={() => handleSiPMClick(arrayIndex, sipmIndex, 0)}
                        >
                            {sipmIndex + 1}
                        </div>
                        <div className="long-bar"></div>
                        <div
                            className={`sipm ${isSelected && formData.selectedSiPMs[arrayIndex][sipmIndex][1] ? 'selected' : ''}`}
                            onClick={() => handleSiPMClick(arrayIndex, sipmIndex, 1)}
                        >
                            {sipmIndex + 1}
                        </div>
                    </div>
                );
            }

            arrays.push(
                <div key={arrayIndex} className="col-sm-2 mx-auto">
                    <button className="btn btn-secondary" onClick={handleSelectArrayAll(arrayIndex)}>
                        {formData.selectedSiPMs[arrayIndex].flat().every((sipm) => sipm ) ? 'Deselect Array' : 'Select Array'}
                    </button>
                    {sipmArray}
                </div>
            );
        }

        return arrays;
    };

    return (
        <form onSubmit={submitFormData}>
            <div className=" ">
                <div className="row">
                    {renderArrays()}
                </div>
                    <div class="d-grid gap-4 col-6 mx-auto">
                    <div className="clearfix">
                            <button
                            className="btn btn-secondary float-start"
                                onClick={handleSelectAll}>
                                {formData.selectedSiPMs.flat().every((sipm) => sipm && sipm[0] && sipm[1]) ? 'Deselect All' : 'Select All'}
                            </button>
                            <button
                            className={`btn float-end ${isAnySiPMSelected ? 'btn-success' : 'btn-danger'} `}
                                type="submit"
                                disabled={!isAnySiPMSelected}
                            >
                                Continue
                            </button>
                        </div>
                    </div>
                </div>
            
        </form>
    );
};

export default SiPMSelector;
