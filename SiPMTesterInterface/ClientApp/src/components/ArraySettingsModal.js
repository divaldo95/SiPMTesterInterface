import React, { useContext, useState, useEffect, useRef } from 'react';
import { Modal, Button, InputGroup, Spinner, Form, Col, Row, FloatingLabel } from 'react-bootstrap';
import { MeasurementContext } from '../context/MeasurementContext';
import ModeSelectButtonGroup from './ModeSelectButtonGroup';

function ArraySettingsModal(props) {
    const { showModal, closeModal, BlockIndex, ModuleIndex, ArrayIndex } = props;
    const [buttonStatus, setButtonStatus] = useState({});
    const [validated, setValidated] = useState(false);
    const { measurementData, updateVoltages, updateVopData, updateBarcode } = useContext(MeasurementContext);
    const inputContainerRef = useRef(null);
    const [isIntervalWaitingUpdate, setIsIntervalWaitingUpdate] = useState(false);
    const [isIntervalUpdateSuccess, setIsIntervalUpdateSuccess] = useState(false);
    const [isIntervalUpdateError, setIsIntervalUpdateError] = useState(false);

    const onSubmit = (event) => {
        event.preventDefault();
        const form = event.currentTarget;
        if (form.checkValidity() === false) {
            event.preventDefault();
            event.stopPropagation();
        } else {
            //handle data fetching by barcode here
        }
        //setValidated(true);
    };

    useEffect(() => {
        const handlePaste = (e) => {
            const clipboardData = e.clipboardData || window.clipboardData;
            const pastedData = clipboardData.getData('Text');
            const rows = pastedData.split('\n').map(row => row.split('\t'));

            rows.forEach((row, rowIndex) => {
                if (rowIndex < measurementData.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs.length) {
                    const value = parseFloat(row[0].replace(',', '.').replace('\r', '').trim());
                    if (!isNaN(value)) {
                        handleVopChange(rowIndex, value);
                    }
                }
            });
        };

        const currentRef = inputContainerRef.current;
        if (currentRef) {
            currentRef.addEventListener('paste', handlePaste);
        }
        return () => {
            if (currentRef) {
                currentRef.removeEventListener('paste', handlePaste);
            }
        };
    }, [showModal]);

    const currentArray = measurementData.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex]; 

    const handleVopChange = (index, value) => {
        const updatedMeasurementData = { ...measurementData };
        console.log(updatedMeasurementData);
        const Vop = parseFloat(value).toFixed(2);
        console.log(Vop);
        //currentArray.OperatingVoltage = newValue.toFixed(2);
        updateVopData(BlockIndex, ModuleIndex, ArrayIndex, index, Vop);
    };

    const getCompensatedVop = (Vop, atTemperature, toTemperature) => {
        const deltaTemperature = atTemperature - toTemperature;
        const compensatedVoltage = parseFloat(Vop) + parseFloat(deltaTemperature * 0.037);
        return compensatedVoltage;
    };

    const handleGenerateVoltageList = (index) => {
        const buttonKey = `pulser-${BlockIndex}-${ModuleIndex}-${ArrayIndex}-${index}`;
        setButtonStatus((prevStatus) => ({ ...prevStatus, [buttonKey]: 'waiting' }));
        const value = measurementData.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs[index].OperatingVoltage;
        if (isNaN(value)) {
            return;
        }
        const compensatedValue = getCompensatedVop(value, 20.0, 25.0);
        const start = compensatedValue.toFixed(2) - 0.5;
        const end = compensatedValue.toFixed(2) + 0.5;
        const step = 0.02;

        if (!isNaN(start) && !isNaN(end) && !isNaN(step) && step > 0) {
            const generatedVoltages = [];
            for (let i = start; i <= end; i += step) {
                generatedVoltages.push(i.toFixed(2));
            }
            sortAndSetVoltageList(generatedVoltages, index);
            setButtonStatus((prevStatus) => ({ ...prevStatus, [buttonKey]: 'success' }));
        }
        else {
            setButtonStatus((prevStatus) => ({ ...prevStatus, [buttonKey]: 'error' }));
        }

        setTimeout(() => {
            setButtonStatus((prevStatus) => ({ ...prevStatus, [buttonKey]: 'primary' }));
        }, 5000);
    };

    const sortAndSetVoltageList = (list, index) => {
        const sortedList = list.map(parseFloat).sort((a, b) => a - b);
        updateVoltages(BlockIndex, ModuleIndex, ArrayIndex, index, "IV", sortedList, false);
    };

    const handleBarcodeChange = (e) => {
        //console.log(e.target.value);
        updateBarcode(BlockIndex, ModuleIndex, ArrayIndex, e.target.value);
    };

    return (
        <Modal show={showModal} onHide={closeModal} centered size="lg" fullscreen={false}>
            <Modal.Header closeButton>
                <Modal.Title>{`Array Settings (Block: ${BlockIndex} Module: ${ModuleIndex} Array: ${ArrayIndex})`}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <div className="text-center d-block d-inline-block" ref={inputContainerRef}>
                    <div className="row justify-content-center">
                        <div className="col mb-3">
                            <ModeSelectButtonGroup BlockIndex={BlockIndex} ModuleIndex={ModuleIndex} ArrayIndex={ArrayIndex}> </ModeSelectButtonGroup>
                        </div>
                    </div>
                    <div className="row justify-content-center">
                        <div className="col mb-3">
                            <Form noValidate validated={validated} onSubmit={onSubmit} className="needs-validation">
                                <InputGroup className="mb-3">
                                    <FloatingLabel controlId="floatingBarcodeInput" label="Array's barcode">
                                        <Form.Control
                                            type="text"
                                            placeholder="Array's barcode"
                                            aria-label="Barcode"
                                            aria-describedby="pulser-submit-btn"
                                            value={currentArray.Barcode}
                                            onChange={handleBarcodeChange}
                                            name="Barcode"
                                            required
                                            isInvalid={validated && currentArray.Barcode.length < 1}
                                        />
                                        <Form.Control.Feedback type="invalid">
                                            Please enter a barcode
                                        </Form.Control.Feedback>
                                    </FloatingLabel>
                                    <Button
                                        className={isIntervalUpdateSuccess ? "btn-success" : isIntervalUpdateError ? "btn-danger" : "btn-primary"}
                                        type="submit"
                                        id="pulser-submit-btn"
                                        disabled={isIntervalWaitingUpdate || isIntervalUpdateSuccess || isIntervalUpdateError}
                                    >
                                        {isIntervalWaitingUpdate ? (
                                            <>
                                                Fetching Vops...
                                                <Spinner animation="border" role="status" size="sm" className="ms-2">
                                                    <span className="visually-hidden">Loading...</span>
                                                </Spinner>
                                            </>
                                        ) : (
                                            isIntervalUpdateSuccess ? "Vops fetched" : isIntervalUpdateError ? "Error fetching" : "Fetch Vops"
                                        )}
                                    </Button>
                                </InputGroup>
                            </Form>
                        </div>
                    </div>
                    <div className="row">
                        <div className="col">
                            <ul className="list-group">
                                {measurementData.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs.map((sipm, index) => (
                                    <InputGroup className="mb-1" key={index}>
                                        <InputGroup.Text>
                                            SiPM {index} Vop
                                        </InputGroup.Text>
                                        <Form.Control
                                            type="number"
                                            placeholder="SiPM operating voltage from datasheet"
                                            aria-label="Operating Voltage"
                                            aria-describedby="Vop-submit-btn"
                                            value={currentArray.SiPMs[index].OperatingVoltage}
                                            onChange={(e) => handleVopChange(index, e.target.value)}
                                            name="Vop"
                                            min="20"
                                            max="45"
                                            step="0.01"
                                            required
                                            isInvalid={validated && (sipm.OperatingVoltage < 20 || sipm.OperatingVoltage > 45)}
                                        />
                                        <Form.Control.Feedback type="invalid">
                                            Please enter a value between 20 and 45.
                                        </Form.Control.Feedback>
                                        <Button
                                            variant={buttonStatus[`pulser-${BlockIndex}-${ModuleIndex}-${ArrayIndex}-${index}`] === 'success' ? "success" : buttonStatus[`pulser-${BlockIndex}-${ModuleIndex}-${ArrayIndex}-${index}`] === 'error' ? "danger" : "primary"}
                                            type="button"
                                            id="pulser-submit-btn"
                                            disabled={buttonStatus[`pulser-${BlockIndex}-${ModuleIndex}-${ArrayIndex}-${index}`] === 'waiting'}
                                            onClick={() => handleGenerateVoltageList(index)}
                                        >
                                            {buttonStatus[`pulser-${BlockIndex}-${ModuleIndex}-${ArrayIndex}-${index}`] === 'waiting' ? (
                                                <>
                                                    <Spinner animation="border" role="status" size="sm" className="ms-2">
                                                        <span className="visually-hidden">Loading...</span>
                                                    </Spinner>
                                                </>
                                            ) : (
                                                buttonStatus[`pulser-${BlockIndex}-${ModuleIndex}-${ArrayIndex}-${index}`] === 'success' ? <i className="bi bi-check2-circle "></i> : buttonStatus[`pulser-${BlockIndex}-${ModuleIndex}-${ArrayIndex}-${index}`] === 'error' ? <i className="bi bi-check2-circle"></i> : <i className="bi bi-check2-circle"></i>
                                            )}
                                        </Button>
                                    </InputGroup>
                                ))}
                            </ul>
                        </div>
                    </div>
                </div>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={closeModal}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
}

export default ArraySettingsModal;
