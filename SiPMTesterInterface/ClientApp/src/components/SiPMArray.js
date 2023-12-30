import React, { useState } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import './SiPMArray.css';

const SiPMArray = ({ numArrays, selectedSiPMs, updateSelectedSiPMs, nextStep, editable }) => {
    const isAnySiPMSelected = selectedSiPMs.some((array) => array.some((sipm) => sipm && (sipm)));

    const toggleSiPM = (arrayIndex, sipmIndex) => {
        if (!editable) {
            return;
        }
        const newSelectedSiPMs = [...selectedSiPMs];
        newSelectedSiPMs[arrayIndex][sipmIndex] = !newSelectedSiPMs[arrayIndex][sipmIndex];
        updateSelectedSiPMs(newSelectedSiPMs);
    };

    const toggleSelectAll = (arrayIndex) => {
        if (!editable) {
            return;
        }
        const newSelectedSiPMs = [...selectedSiPMs];
        newSelectedSiPMs[arrayIndex] = newSelectedSiPMs[arrayIndex].map((value) => !value);
        updateSelectedSiPMs(newSelectedSiPMs);
    };

    const toggleMasterSelectAll = () => {
        if (!editable) {
            return;
        }
        const areAllSelected = selectedSiPMs.every((array) => array.every((value) => value));

        const newSelectedSiPMs = selectedSiPMs.map((array) => {
            return array.map(() => !areAllSelected);
        });

        updateSelectedSiPMs(newSelectedSiPMs);
    };

    const isSelectAll = (arrayIndex) => selectedSiPMs[arrayIndex].every((value) => value);
    const isMasterSelectAll = () => selectedSiPMs.every((array) => array.every((value) => value));

    const handleSubmit = (e) => {
        e.preventDefault();
        // Handle form submission if needed
        nextStep();
    };

    return (
        <form onSubmit={handleSubmit}>
            <div>
                <div className="d-flex flex-wrap justify-content-center">
                    {Array.from({ length: numArrays }, (_, i) => (
                        <div key={i} className="card m-2">
                            <h4 className="card-header d-flex justify-content-between align-items-center">
                                Array {i}:
                                <button onClick={(e) => { e.preventDefault(); toggleSelectAll(i); }} className="btn btn-secondary float-right">
                                    {isSelectAll(i) ? 'Deselect All' : 'Select All'}
                                </button>
                            </h4>
                            <div className="card-body d-flex flex-wrap">
                                {Array.from({ length: 16 }, (_, j) => (
                                    <div
                                        key={j}
                                        className={`sipm-box ${selectedSiPMs[i][j] ? 'bg-success' : 'bg-light'}`}
                                        onClick={() => toggleSiPM(i, j)}
                                    >
                                        {j}
                                    </div>
                                ))}
                            </div>
                        </div>
                    ))}
                </div>

                <div className="d-grid gap-4 col-6 mx-auto">
                    <div className="clearfix">
                        <button onClick={(e) => { e.preventDefault(); toggleMasterSelectAll(); }} className="btn btn-primary float-start">
                            {isMasterSelectAll() ? 'Deselect All' : 'Select All'}
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

export default SiPMArray;
