import React, { useState } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import './SiPMArray.css';

const SiPMArray = ({ numArrays, formData, updateSelectedSiPMs, updateIVMeas, updateSPSMeas, nextStep, editable }) => {
    const isAnySiPMSelected = formData.selectedSiPMs.some((array) => array.some((sipm) => sipm && (sipm))) && (formData.ivMeasurementEnabled || formData.spsMeasurementEnabled);

    const toggleSiPM = (arrayIndex, sipmIndex) => {
        if (!editable) {
            return;
        }
        const newSelectedSiPMs = [...formData.selectedSiPMs];
        newSelectedSiPMs[arrayIndex][sipmIndex] = !newSelectedSiPMs[arrayIndex][sipmIndex];
        updateSelectedSiPMs(newSelectedSiPMs);
    };

    const toggleIV = () => {
        if (!editable) {
            return;
        }
        const newIVMode = formData.ivMeasurementEnabled;
        updateIVMeas(!newIVMode);
    };

    const toggleSPS = () => {
        if (!editable) {
            return;
        }
        const newSPSMode = formData.spsMeasurementEnabled;
        updateSPSMeas(!newSPSMode);
    };

    const toggleSelectAll = (arrayIndex) => {
        if (!editable) {
            return;
        }
        const newSelectedSiPMs = [...formData.selectedSiPMs];
        const isEverySelected = formData.selectedSiPMs[arrayIndex].every((sipm) => sipm && (sipm));
        newSelectedSiPMs[arrayIndex] = newSelectedSiPMs[arrayIndex].map((value) => !isEverySelected);
        updateSelectedSiPMs(newSelectedSiPMs);
    };

    const toggleMasterSelectAll = () => {
        if (!editable) {
            return;
        }
        const areAllSelected = formData.selectedSiPMs.every((array) => array.every((value) => value));

        const newSelectedSiPMs = formData.selectedSiPMs.map((array) => {
            return array.map(() => !areAllSelected);
        });

        updateSelectedSiPMs(newSelectedSiPMs);
    };

    const isSelectAll = (arrayIndex) => formData.selectedSiPMs[arrayIndex].every((value) => value);
    const isMasterSelectAll = () => formData.selectedSiPMs.every((array) => array.every((value) => value));

    const handleSubmit = (e) => {
        e.preventDefault();
        // Handle form submission if needed
        nextStep();
    };

    return (
        <form onSubmit={handleSubmit}>

            <div className="row justify-content-center mb-4">
                <div className="col-md-8">
                    <div className="row">
                        <div className="col">
                            <div className="d-flex flex-column h-100">
                                <div className="card flex-grow-1">

                                    <div className="card-header align-items-center">
                                        <div class="row align-items-center">
                                            <h4 class="col-md-11">
                                                Measurement modes:
                                            </h4>

                                        </div>
                                    </div>
                                    <div className="card-body">
                                        <div className="d-flex justify-content-center gap-4">
                                            <div className="d-flex row">
                                                <div className="col">
                                                    <div className="row">
                                                        <div className="col d-flex">
                                                            <input
                                                                type="checkbox"
                                                                className="btn-check align-self-center"
                                                                id="btn-check-outlined-iv"
                                                                autoComplete="off"
                                                                checked={formData.ivMeasurementEnabled}
                                                                onChange={toggleIV}
                                                            />
                                                            <label
                                                                className={`btn ${formData.ivMeasurementEnabled ? 'btn-outline-success' : 'btn-outline-secondary'} flex-grow-1 d-flex align-items-center justify-content-center`}
                                                                htmlFor="btn-check-outlined-iv">
                                                                IV measurement
                                                            </label>
                                                        </div>
                                                        <div className="col d-flex">
                                                            <input
                                                                type="checkbox"
                                                                className="btn-check align-self-center"
                                                                id="btn-check-outlined-sps"
                                                                autoComplete="off"
                                                                checked={formData.spsMeasurementEnabled}
                                                                onChange={toggleSPS}
                                                            />
                                                            <label
                                                                className={`btn ${formData.spsMeasurementEnabled ? 'btn-outline-success' : 'btn-outline-secondary'} flex-grow-1 d-flex align-items-center justify-content-center`}
                                                                htmlFor="btn-check-outlined-sps">
                                                                SPS measurement
                                                            </label>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>


                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div className="d-flex flex-wrap gap-4">
                <div className="d-flex flex-wrap justify-content-center">
                    {Array.from({ length: numArrays }, (_, i) => (
                        <div key={i} className="card m-2 col-md-8">
                            <h4 className="card-header d-flex justify-content-between align-items-center">
                                Array {i}:
                                {editable ? (
                                    <button onClick={(e) => { e.preventDefault(); toggleSelectAll(i); }} className="btn btn-secondary float-right">
                                        {isSelectAll(i) ? 'Deselect All' : 'Select All'}
                                    </button>
                                ) :
                                    (<div className="float-right">{formData.barcodes[i]}</div>)}
                            </h4>
                            <div className="card-body d-flex flex-wrap justify-content-center">
                                {Array.from({ length: 16 }, (_, j) => (
                                    <div
                                        key={j}
                                        className={`sipm-box ${formData.selectedSiPMs[i][j] ? 'bg-success' : 'bg-light'}`}
                                        onClick={() => toggleSiPM(i, j)}
                                    >
                                        {j}
                                    </div>
                                ))}
                            </div>
                        </div>
                    ))}
                </div>

                {editable && (
                    <div className="d-grid gap-4 col-6 mx-auto">
                        <div className="clearfix  mb-3">
                            <button onClick={(e) => { e.preventDefault(); toggleMasterSelectAll(); }} className="btn btn-primary float-start">
                                {isMasterSelectAll() ? 'Deselect All' : 'Select All'}
                            </button>
                            <button
                                className={`btn float-end ${isAnySiPMSelected ? 'btn-success' : 'btn-danger'}`}
                                type="submit"
                                disabled={!isAnySiPMSelected}
                            >
                                Continue
                            </button>
                        </div>
                    </div>
                )}
            </div>
        </form>
    );
};

export default SiPMArray;
