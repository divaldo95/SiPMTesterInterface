import React, { useState, useEffect, useRef } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';

const VoltageList = ({ updateVoltageList, prevStep, nextStep, startValue: initialStartValue, endValue: initialEndValue, stepValue: initialStepValue, savedVoltageList }) => {
    const [startValue, setStartValue] = useState(initialStartValue || '');
    const [endValue, setEndValue] = useState(initialEndValue || '');
    const [stepValue, setStepValue] = useState(initialStepValue || '');
    const [voltageList, setVoltageList] = useState([]);
    const fileInputRef = useRef(null);

    useEffect(() => {
        // If the initial values change, update the state
        setStartValue(initialStartValue || '');
        setEndValue(initialEndValue || '');
        setStepValue(initialStepValue || '');
        setVoltageList(savedVoltageList || []); // Set the saved voltage list initially
    }, [initialStartValue, initialEndValue, initialStepValue, savedVoltageList]);

    const sortAndSetVoltageList = (list) => {
        const sortedList = list.map(parseFloat).sort((a, b) => a - b);
        setVoltageList(sortedList);
        updateVoltageList(sortedList);
    };

    const handleGenerationDataChange = (index, value) => {

        //setGenerateData(start, end, step);
    };

    const handleGenerate = (e) => {
        e.preventDefault();

        const start = parseFloat(startValue);
        const end = parseFloat(endValue);
        const step = parseFloat(stepValue);

        if (!isNaN(start) && !isNaN(end) && !isNaN(step) && step > 0) {
            const generatedVoltages = [];
            for (let i = start; i <= end; i += step) {
                generatedVoltages.push(i.toFixed(2));
            }
            sortAndSetVoltageList(generatedVoltages);
        }
    };

    const handleAddLine = (e) => {
        e.preventDefault();
        sortAndSetVoltageList([...voltageList, 0]);
    };

    const emptyVoltageList = (e) => {
        e.preventDefault();
        sortAndSetVoltageList([]);
    };

    const handleRemoveLine = (e, index) => {
        e.preventDefault();
        const updatedList = [...voltageList];
        updatedList.splice(index, 1);
        sortAndSetVoltageList(updatedList);
    };

    const handleLineChange = (index, value) => {
        // No need to preventDefault for onBlur
        const updatedList = [...voltageList];
        updatedList[index] = value;
        // Not sorting here to avoid reordering on every input change
        // Sorting will be done onBlur
        setVoltageList(updatedList);
    };

    const handleLineBlur = () => {
        // Sort the list when an input loses focus
        sortAndSetVoltageList(voltageList);
    };

    const handleImport = (event) => {
        event.preventDefault();
        const file = event.target.files[0];

        if (file) {
            const reader = new FileReader();

            reader.onload = (e) => {
                try {
                    const importedList = JSON.parse(e.target.result);
                    sortAndSetVoltageList(importedList);
                } catch (error) {
                    console.error('Error parsing JSON file:', error);
                }
            };

            reader.readAsText(file);
        }
    };

    const handleExport = () => {
        const exportData = JSON.stringify(voltageList, null, 2);
        const blob = new Blob([exportData], { type: 'application/json' });
        const url = URL.createObjectURL(blob);

        const a = document.createElement('a');
        a.href = url;
        a.download = 'voltage_list.json';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    };

    const handleDrop = (event) => {
        event.preventDefault();
        const file = event.dataTransfer.files[0];

        if (file) {
            const reader = new FileReader();

            reader.onload = (e) => {
                try {
                    const importedList = JSON.parse(e.target.result);
                    sortAndSetVoltageList(importedList);
                } catch (error) {
                    console.error('Error parsing JSON file:', error);
                }
            };

            reader.readAsText(file);
        }
    };

    const handleDragOver = (event) => {
        event.preventDefault();
    };

    const handleDragAndDropClick = () => {
        fileInputRef.current.click();
    };

    const isContinueButtonDisabled = () => {
        return voltageList.length === 0;
    };

    return (
        <div className="container">
            <form onSubmit={nextStep}>
                <div className="row justify-content-center mb-4">
                    <div className="col-md-8">
                        <div className="card">
                            <h5 className="card-header">Voltage List Generator</h5>
                            <div className="card-body">
                                <div className="row">
                                    <div className="col">
                                        <label htmlFor="startInput" className="form-label">
                                            Start Voltage
                                        </label>
                                        <input
                                            type="number"
                                            min="0"
                                            step="0.01"
                                            className="form-control"
                                            id="startInput"
                                            value={startValue}
                                            onChange={(e) => setStartValue(e.target.value)}
                                        />
                                    </div>
                                    
                                    <div className="col">
                                        <label htmlFor="endInput" className="form-label">
                                            End Voltage
                                        </label>
                                        <input
                                            type="number"
                                            min="0"
                                            step="0.01"
                                            className="form-control"
                                            id="endInput"
                                            value={endValue}
                                            onChange={(e) => setEndValue(e.target.value)}
                                        />
                                    </div>

                                    <div className="col">
                                        <label htmlFor="stepInput" className="form-label">
                                            Step Value
                                        </label>
                                        <input
                                            type="number"
                                            min="0.01"
                                            step="0.01"
                                            className="form-control"
                                            id="stepInput"
                                            value={stepValue}
                                            onChange={(e) => setStepValue(e.target.value)}
                                        />
                                    </div>
                                </div>
                                <div className="container">
                                    <div className="row text-center">
                                        <div className="col p-3">
                                            <button className="btn btn-primary"
                                                onClick={handleGenerate}
                                            >
                                                Generate
                                            </button>
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
                                        <h5 className="card-header">Export voltage list</h5>
                                        <div className="card-body">
                                            <div className="d-flex justify-content-center">
                                                <button className="btn btn-primary" onClick={handleExport}>
                                                    Export
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div className="col">
                                <div className="d-flex flex-column h-100">
                                    <div className="card flex-grow-1">
                                        <h5 className="card-header">Import voltage list</h5>
                                        <div className="card-body">
                                            <div className="d-none justify-content-center">
                                                <input
                                                    type="file"
                                                    accept=".json"
                                                    ref={fileInputRef}
                                                    style={{ display: 'none' }}
                                                    onChange={handleImport}
                                                />
                                            </div>
                                            <div
                                                className="border text-center"
                                                onClick={handleDragAndDropClick}
                                                onDrop={handleDrop}
                                                onDragOver={handleDragOver}
                                                style={{ cursor: 'pointer' }}
                                            >
                                                Click or drag and drop a JSON file to import
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
                            <h5 className="card-header d-flex justify-content-between align-items-center">
                                Voltages
                                <div>
                                    <button className="btn btn-danger btn-sm mx-1" onClick={emptyVoltageList}>
                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-trash-fill" viewBox="0 0 16 16">
                                            <path d="M2.5 1a1 1 0 0 0-1 1v1a1 1 0 0 0 1 1H3v9a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2V4h.5a1 1 0 0 0 1-1V2a1 1 0 0 0-1-1H10a1 1 0 0 0-1-1H7a1 1 0 0 0-1 1zm3 4a.5.5 0 0 1 .5.5v7a.5.5 0 0 1-1 0v-7a.5.5 0 0 1 .5-.5M8 5a.5.5 0 0 1 .5.5v7a.5.5 0 0 1-1 0v-7A.5.5 0 0 1 8 5m3 .5v7a.5.5 0 0 1-1 0v-7a.5.5 0 0 1 1 0" />
                                        </svg>
                                    </button>
                                    <button className="btn btn-success btn-sm" onClick={handleAddLine}>
                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-plus" viewBox="0 0 16 16">
                                            <path d="M8 4a.5.5 0 0 1 .5.5v3h3a.5.5 0 0 1 0 1h-3v3a.5.5 0 0 1-1 0v-3h-3a.5.5 0 0 1 0-1h3v-3A.5.5 0 0 1 8 4" />
                                        </svg>
                                    </button>
                                </div>
                            </h5>
                            <div className="card-body">
                                <ul className="list-group">
                                    {voltageList.map((voltage, index) => (
                                        <li key={index} className="input-group-sm mb-1 d-flex justify-content-between align-items-center">
                                            <input
                                                type="number"
                                                min="0"
                                                step="0.01"
                                                className="form-control"
                                                value={voltage}
                                                onChange={(e) => handleLineChange(index, e.target.value)}
                                                onBlur={handleLineBlur}
                                            />
                                            <button className="btn btn-danger" onClick={(e) => handleRemoveLine(e, index)}>
                                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-trash" viewBox="0 0 16 16">
                                                    <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5m2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5m3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0z" />
                                                    <path d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4zM2.5 3h11V2h-11z" />
                                                </svg>
                                            </button>
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="d-grid gap-4 col-6 mx-auto">
                    <div className="clearfix">
                        <button
                            className="btn btn-secondary float-start"
                            onClick={prevStep}
                        >
                            Back
                        </button>
                        <button
                            className="btn float-end btn-success"
                            type="submit"
                            disabled={isContinueButtonDisabled()}
                        >
                            Continue
                        </button>
                    </div>
                </div>
            </form >
         </div>
        
    );
};

export default VoltageList;
