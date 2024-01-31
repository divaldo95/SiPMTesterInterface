// SiPMSelector.js
import React, { useState } from 'react';
import './SiPMSelector.css';
import BarcodeInput from './BarcodeInput';

const SiPMSelector = () => {
    const [selectedSiPMs, setSelectedSiPMs] = useState(Array.from({ length: 4 }, () => Array.from({ length: 16 }, () => [true, true])));
    const [showBarcodeInput, setShowBarcodeInput] = useState(false);
    const [currentStep, setCurrentStep] = useState(1);
    const isAnySiPMSelected = selectedSiPMs.some((array) => array.some((sipm) => sipm && (sipm[0] || sipm[1])));

    const handleSiPMClick = (arrayIndex, sipmIndex, boxIndex) => {
        const newSelectedSiPMs = [...selectedSiPMs];
        newSelectedSiPMs[arrayIndex] = newSelectedSiPMs[arrayIndex] || [];
        newSelectedSiPMs[arrayIndex][sipmIndex] = newSelectedSiPMs[arrayIndex][sipmIndex] || [];

        // Toggle the selected state of the clicked box
        newSelectedSiPMs[arrayIndex][sipmIndex][boxIndex] = !newSelectedSiPMs[arrayIndex][sipmIndex][boxIndex];

        setSelectedSiPMs(newSelectedSiPMs);
    };

    const handleSelectArrayAll = (arrayIndex) => {
        // Toggle select all state for the specific array
        const newSelectedSiPMs = [...selectedSiPMs];
        newSelectedSiPMs[arrayIndex] = newSelectedSiPMs[arrayIndex] || [];
        const selectAllState = newSelectedSiPMs[arrayIndex].every((sipm) => sipm && sipm[0] && sipm[1]);

        newSelectedSiPMs[arrayIndex] = Array.from({ length: 16 }, () => [!selectAllState, !selectAllState]);
        setSelectedSiPMs(newSelectedSiPMs);
    };

    const handleSelectAll = () => {
        // Toggle select all state for all arrays
        const selectAllState = selectedSiPMs.flat().every((sipm) => sipm && sipm[0] && sipm[1]);

        const newSelectedSiPMs = Array.from({ length: 4 }, () =>
            Array.from({ length: 16 }, () => [!selectAllState, !selectAllState])
        );

        setSelectedSiPMs(newSelectedSiPMs);
    };

    const handleContinue = () => {
        if (currentStep === 1) {
            setShowBarcodeInput(true);
        } else {
            // Handle additional steps if needed
            // For now, let's assume there are only 2 steps
            setCurrentStep(currentStep + 1);
        }
    };

    const handleBack = () => {
        setShowBarcodeInput(false);
    };

    const handleMeasure = () => {
        // Collect data for measurement
        const selectedIndexes = [];
        selectedSiPMs.forEach((array, arrayIndex) => {
            array.forEach((sipm, sipmIndex) => {
                if (sipm && sipm[0]) {
                    selectedIndexes.push({
                        arrayIndex,
                        sipmIndex,
                    });
                }
            });
        });

        // Collect barcode data (assuming you have a state for barcodes)
        // const barcodeData = ...

        // Perform the measurement or further action with the collected data
        console.log('Selected SiPMs Indexes:', selectedIndexes);
        // console.log('Barcode Data:', barcodeData);
    };

    const renderArrays = () => {
        const arrays = [];

        for (let arrayIndex = 0; arrayIndex < 4; arrayIndex++) {
            const sipmArray = [];

            for (let sipmIndex = 0; sipmIndex < 16; sipmIndex++) {
                const isSelected = selectedSiPMs[arrayIndex] && selectedSiPMs[arrayIndex][sipmIndex];

                sipmArray.push(
                    <div key={sipmIndex} className="sipm-container">
                        <div
                            className={`sipm ${isSelected && selectedSiPMs[arrayIndex][sipmIndex][0] ? 'selected' : ''}`}
                            onClick={() => handleSiPMClick(arrayIndex, sipmIndex, 0)}
                        >
                            {sipmIndex + 1}
                        </div>
                        <div className="long-bar"></div>
                        <div
                            className={`sipm ${isSelected && selectedSiPMs[arrayIndex][sipmIndex][1] ? 'selected' : ''}`}
                            onClick={() => handleSiPMClick(arrayIndex, sipmIndex, 1)}
                        >
                            {sipmIndex + 1}
                        </div>
                    </div>
                );
            }

            arrays.push(
                <div key={arrayIndex} className="sipm-array">
                    <button className="btn btn-secondary" onClick={() => handleSelectArrayAll(arrayIndex)}>
                        {selectedSiPMs[arrayIndex].flat().every((sipm) => sipm ) ? 'Deselect Array' : 'Select Array'}
                    </button>
                    {sipmArray}
                </div>
            );
        }

        return arrays;
    };

    return (
        <div>
            {showBarcodeInput ? (
                <BarcodeInput onBack={handleBack} />
            ) : (
                <div className="sipm-selector row">
                    <div className="array-container">
                        {renderArrays()}
                    </div>
                        <div class="d-grid gap-4 col-4 mx-auto">
                            <div className="col">
                                <button
                                    className="btn btn-primary me-3"
                                    onClick={handleSelectAll}>
                                    {selectedSiPMs.flat().every((sipm) => sipm && sipm[0] && sipm[1]) ? 'Deselect All' : 'Select All'}
                                </button>
                                {currentStep === 1 && (
                                    <button
                                        className={`btn me-3 ${isAnySiPMSelected ? 'btn-success' : 'btn-danger'} `}
                                        onClick={handleContinue}
                                        disabled={!isAnySiPMSelected}
                                    >
                                        Continue
                                    </button>
                                )}
                                {currentStep === 2 && (
                                    <button className="btn btn-primary measure-button" onClick={handleMeasure}>
                                        Measure
                                    </button>
                                )}
                            </div>
                        </div>
                    
                </div>
            )}
        </div>
    );
};

export default SiPMSelector;
