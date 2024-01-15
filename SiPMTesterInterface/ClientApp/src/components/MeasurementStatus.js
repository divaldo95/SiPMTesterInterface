
import React, { useState, useEffect, useRef } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import './SiPMArray.css';
import SiPMModal from './SiPMModal';

const MeasurementStatus = ({ numArrays, formData, measurementStates }) => {
    const [showModal, setShowModal] = useState(false);
    const [selectedArray, setSelectedArray] = useState(null);
    const [selectedSiPM, setSelectedSiPM] = useState(null);

    const openModal = (arrayNumber, sipmNumber) => {
        setSelectedArray(arrayNumber);
        setSelectedSiPM(sipmNumber);
        setShowModal(true);
    };

    const closeModal = () => {
        setShowModal(false);
    };

    return (
        <div className="container">
            <form onSubmit={() => { }}>
                <div className="row justify-content-center mb-4">
                    <div className="col-md-8">
                        <div className="card">
                            <h5 className="card-header">STOP</h5>
                            <div className="card-body">
                                <div className="container">
                                    <div className="row text-center">
                                        <div className="col p-3">
                                            <div class="d-grid gap-2">
                                                <button className="btn btn-lg btn-danger"
                                                    onClick={() => { }}
                                                >
                                                    STOP
                                                </button>
                                            </div>
                                            
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div className="row justify-content-center mb-4">
                    <div className="col-md-8">
                        <div className="row">
                            <div className="col">
                                <div className="d-flex flex-column h-100">
                                    <div className="card flex-grow-1">
                                        <h5 className="card-header d-flex justify-content-between align-items-center">
                                            Current IV measurements
                                            <div className="float-right">
                                                {measurementStates.ivState !== 0 && (
                                                    <div class="spinner-border" role="status">
                                                        <span class="sr-only"></span>
                                                    </div>
                                                )}
                                            </div>
                                        </h5>
                                        <div className="card-body">
                                            <div className="d-grid">
                                                <ul class="list-group">
                                                    <li class="list-group-item">
                                                        <div class="d-flex justify-content-center align-items-center">
                                                            <span class="badge bg-secondary">Array 0</span>
                                                            <span class="mx-2"> - </span>
                                                            <span class="badge bg-primary">SiPM 0</span>
                                                        </div>
                                                    </li>
                                                </ul>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div className="col">
                                <div className="d-flex flex-column h-100">
                                    <div className="card flex-grow-1">
                                        <h5 className="card-header d-flex justify-content-between align-items-center">
                                            Current SPS measurements
                                            <div className="float-right">
                                                {measurementStates.spsState !== 0 && (
                                                    <div class="spinner-border" role="status">
                                                        <span class="sr-only"></span>
                                                    </div>
                                                )}
                                            </div>
                                        </h5>
                                        <div className="card-body">
                                            <div className="d-grid">
                                                <ul class="list-group">
                                                    
                                                    <li class="list-group-item">
                                                        <div class="d-flex justify-content-center align-items-center">
                                                            <span class="badge bg-secondary">Array 0</span>
                                                            <span class="mx-2"> - </span>
                                                            <span class="badge bg-primary">SiPM 0</span>
                                                        </div>
                                                    </li>
                                                    <li class="list-group-item">
                                                        <div class="d-flex justify-content-center align-items-center">
                                                            <span class="badge bg-secondary">Array 0</span>
                                                            <span class="mx-2"> - </span>
                                                            <span class="badge bg-primary">SiPM 0</span>
                                                        </div>
                                                    </li>
                                                    <li class="list-group-item">
                                                        <div class="d-flex justify-content-center align-items-center">
                                                            <span class="badge bg-secondary">Array 0</span>
                                                            <span class="mx-2"> - </span>
                                                            <span class="badge bg-primary">SiPM 0</span>
                                                        </div>
                                                    </li>
                                                    <li class="list-group-item">
                                                        <div class="d-flex justify-content-center align-items-center">
                                                            <span class="badge bg-secondary">Array 0</span>
                                                            <span class="mx-2"> - </span>
                                                            <span class="badge bg-primary">SiPM 0</span>
                                                        </div>
                                                    </li>
                                                    <li class="list-group-item">
                                                        <div class="d-flex justify-content-center align-items-center">
                                                            <span class="badge bg-secondary">Array 0</span>
                                                            <span class="mx-2"> - </span>
                                                            <span class="badge bg-primary">SiPM 0</span>
                                                        </div>
                                                    </li>
                                                    <li class="list-group-item">
                                                        <div class="d-flex justify-content-center align-items-center">
                                                            <span class="badge bg-secondary">Array 0</span>
                                                            <span class="mx-2"> - </span>
                                                            <span class="badge bg-primary">SiPM 0</span>
                                                        </div>
                                                    </li>
                                                    <li class="list-group-item">
                                                        <div class="d-flex justify-content-center align-items-center">
                                                            <span class="badge bg-secondary">Array 0</span>
                                                            <span class="mx-2"> - </span>
                                                            <span class="badge bg-primary">SiPM 0</span>
                                                        </div>
                                                    </li>
                                                    <li class="list-group-item">
                                                        <div class="d-flex justify-content-center align-items-center">
                                                            <span class="badge bg-secondary">Array 0</span>
                                                            <span class="mx-2"> - </span>
                                                            <span class="badge bg-primary">SiPM 0</span>
                                                        </div>
                                                    </li>
                                                </ul>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div className="row justify-content-center mb-4">
                    <div className="col-md-8">
                        <div className="card">
                            <h5 className="card-header">Color legend</h5>
                            <div className="card-body">
                                <div className="container">
                                    <div className="row text-center">
                                        <div className="col">
                                            <div class="d-grid">
                                                <div class="d-flex justify-content-center align-items-center gap-2">
                                                    <h4>
                                                        <span class="badge bg-secondary">Selected</span>
                                                    </h4>
                                                    <h4>
                                                        <span class="badge bg-light text-dark">Not selected</span>
                                                    </h4>
                                                    <h4>
                                                        <span class="badge bg-primary">Under test</span>
                                                    </h4>
                                                    <h4>
                                                        <span class="badge bg-danger">Error</span>
                                                    </h4>
                                                    <h4>
                                                        <span class="badge bg-success">Success</span>
                                                    </h4>
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
                                    {<div className="float-right">{formData.barcodes[i]}</div>}
                                </h4>
                                <div className="card-body d-flex flex-wrap justify-content-center">
                                    {Array.from({ length: 16 }, (_, j) => (
                                        <div
                                            key={j}
                                            className={`sipm-box ${formData.selectedSiPMs[i][j] ? 'bg-success' : 'bg-light'}`}
                                            onClick={() => openModal(i, j)}
                                        >
                                            {j}
                                        </div>
                                    ))}
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
                <SiPMModal showModal={showModal} closeModal={closeModal} arrayNumber={selectedArray} sipmNumber={selectedSiPM} />
            </form >
        </div>

    );
};
 
export default MeasurementStatus;
