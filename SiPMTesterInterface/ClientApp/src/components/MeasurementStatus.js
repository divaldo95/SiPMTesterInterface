
import React, { useState, useEffect, useRef } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';

const MeasurementStatus = ({ numArrays, formData }) => {

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
                                                <div class="spinner-border" role="status">
                                                    <span class="sr-only"></span>
                                                </div>
                                            </div>
                                        </h5>
                                        <div className="card-body">
                                            <div className="d-grid">
                                                <ul class="list-group">
                                                    <li class="list-group-item">Cras justo odio</li>
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
                                                <div class="spinner-border" role="status">
                                                    <span class="sr-only"></span>
                                                </div>
                                            </div>
                                        </h5>
                                        <div className="card-body">
                                            <div className="d-grid">
                                                <ul class="list-group">
                                                    <li class="list-group-item">Cras justo odio</li>
                                                    <li class="list-group-item">Dapibus ac facilisis in</li>
                                                    <li class="list-group-item">Morbi leo risus</li>
                                                    <li class="list-group-item">Porta ac consectetur ac</li>
                                                    <li class="list-group-item">Vestibulum at eros</li>
                                                </ul>
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
                                            onClick={() => { } }
                                        >
                                            {j}
                                        </div>
                                    ))}
                                </div>
                            </div>
                        ))}
                    </div>
                 </div>

                <div class="d-grid gap-4 col-6 mx-auto">
                    <div className="clearfix mb-3">
                        <button
                            className="btn btn-secondary float-start"
                        >
                            Back
                        </button>
                        <button
                            className={`btn float-end ${!false ? 'btn-success' : 'btn-danger'}`}
                            type="submit"
                            disabled={false}
                        >
                            Continue
                        </button>
                    </div>
                </div>
            </form >
        </div>

    );
};
 
export default MeasurementStatus;
