import React, { useState, useEffect } from 'react';
import { Accordion, Card, Button, Form, Modal, InputGroup, FloatingLabel, Spinner } from 'react-bootstrap';
import MeasurementStateService from '../services/MeasurementStateService';


const PulserValuesModal = ({ show, handleClose }) => {
    const initialPulserData = {
        Blocks: [
        {
            Modules: [
            {
                Arrays: [
                {
                    SiPMs: [
                    { PulserValue: 100 }
                    ],
                    LEDPulserOffset: 10
                }
                ]
            }
            ]
        }
        ]
    };

    

    const [pulserData, setPulserData] = useState(initialPulserData);
    const [buttonStatus, setButtonStatus] = useState({});
    const [validated, setValidated] = useState(false);

    const exportToJson = () => {

    }

    const getPulserLEDValues = async () => {
        try {
            const data = await MeasurementStateService.getPulserLEDValues();
            setPulserData(data);
            console.log(data);
        } catch (error) {
            console.log(error);
        }
    };

    useEffect(() => {
        getPulserLEDValues();
    }, []);

    const handleLineChange = (blockIndex, moduleIndex, arrayIndex, sipmIndex, newValue) => {
        const newData = { ...pulserData };
        newData.Blocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].SiPMs[sipmIndex].PulserValue = parseInt(newValue, 10);
        setPulserData(newData);
    };

    const handleOffsetChange = (blockIndex, moduleIndex, arrayIndex, newValue) => {
        const newData = { ...pulserData };
        newData.Blocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].LEDPulserOffset = parseInt(newValue, 10);
        setPulserData(newData);
    };

    const handleSetPulserValue = async (blockIndex, moduleIndex, arrayIndex, sipmIndex) => {
        const buttonKey = `pulser-${blockIndex}-${moduleIndex}-${arrayIndex}-${sipmIndex}`;
        setButtonStatus((prevStatus) => ({ ...prevStatus, [buttonKey]: 'waiting' }));

        try {
            const updatedSiPM = pulserData.Blocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].SiPMs[sipmIndex];
            MeasurementStateService.setPulserLEDValue(blockIndex, moduleIndex, arrayIndex, sipmIndex, updatedSiPM.PulserValue);
            setButtonStatus((prevStatus) => ({ ...prevStatus, [buttonKey]: 'success' }));
        } catch (error) {
            setButtonStatus((prevStatus) => ({ ...prevStatus, [buttonKey]: 'error' }));
        }

        setTimeout(() => {
            setButtonStatus((prevStatus) => ({ ...prevStatus, [buttonKey]: 'primary' }));
        }, 5000);
    };

    const handleSetLEDPulserOffset = async (blockIndex, moduleIndex, arrayIndex) => {
        const buttonKey = `offset-${blockIndex}-${moduleIndex}-${arrayIndex}`;
        setButtonStatus((prevStatus) => ({ ...prevStatus, [buttonKey]: 'waiting' }));

        try {
            const updatedArray = pulserData.Blocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex];
            MeasurementStateService.setArrayLEDOffset(blockIndex, moduleIndex, arrayIndex, updatedArray.LEDPulserOffset);
            setButtonStatus((prevStatus) => ({ ...prevStatus, [buttonKey]: 'success' }));
        } catch (error) {
            setButtonStatus((prevStatus) => ({ ...prevStatus, [buttonKey]: 'error' }));
        }

        setTimeout(() => {
            setButtonStatus((prevStatus) => ({ ...prevStatus, [buttonKey]: 'primary' }));
        }, 5000);
    };

    const generateLEDValuesJSONString = () => {
        const exportedData = [];

        pulserData.Blocks.forEach((block, blockIndex) => {
            block.Modules.forEach((module, moduleIndex) => {
                module.Arrays.forEach((array, arrayIndex) => {
                    array.SiPMs.forEach((sipm, sipmIndex) => {
                        exportedData.push({
                            BlockIndex: blockIndex,
                            ModuleIndex: moduleIndex,
                            ArrayIndex: arrayIndex,
                            SiPMIndex: sipmIndex,
                            PulserValue: sipm.PulserValue,
                        });
                    });
                });
            });
        });

        return JSON.stringify(exportedData, null, 2);
    };

    // Function to handle download button click
    const handleDownloadLEDValuesClick = () => {
        const jsonString = generateLEDValuesJSONString();
        const blob = new Blob([jsonString], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'ledValues.json';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    };

    const generateArrayOffsetsJSONString = () => {
        const exportedData = [];

        pulserData.Blocks.forEach((block, blockIndex) => {
            block.Modules.forEach((module, moduleIndex) => {
                module.Arrays.forEach((array, arrayIndex) => {
                    exportedData.push({
                        BlockIndex: blockIndex,
                        ModuleIndex: moduleIndex,
                        ArrayIndex: arrayIndex,
                        PulserValue: array.LEDPulserOffset,
                    });
                });
            });
        });

        return JSON.stringify(exportedData, null, 2);
    };

    // Function to handle download button click
    const handleDownloadArrayOffsetsClick = () => {
        const jsonString = generateArrayOffsetsJSONString();
        const blob = new Blob([jsonString], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'ledOffsets.json';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    };

    if (!show) {
        return null;
    }

    return (
        <Modal scrollable show={show} onHide={handleClose} backdrop="static" centered size="lg">
            <Modal.Header closeButton>
                <Modal.Title className="w-100">
                    <div className="d-flex align-items-center justify-content-between">
                        <div className="flex-grow-1 text-truncate">
                            LED Pulser Values
                        </div>
                    </div>
                </Modal.Title>
            </Modal.Header>
            <Modal.Body className="text-center">
                <Accordion>
                    {pulserData.Blocks.map((block, blockIndex) => (
                        <Accordion.Item eventKey={`block-${blockIndex}`} key={blockIndex}>
                            <Accordion.Header>Block {blockIndex }</Accordion.Header>
                            <Accordion.Body className="p-0">
                                {block.Modules.map((module, moduleIndex) => (
                                    <Accordion key={moduleIndex}>
                                        <Accordion.Item eventKey={`module-${moduleIndex}`} key={moduleIndex}>
                                            <Accordion.Header>Module {moduleIndex }</Accordion.Header>
                                            <Accordion.Body className="p-0">
                                                {module.Arrays.map((array, arrayIndex) => (
                                                    <Accordion className="p-0" key={arrayIndex}>
                                                        <Accordion.Item eventKey={`array-${arrayIndex}`} key={arrayIndex}>
                                                            <Accordion.Header>Array {arrayIndex }</Accordion.Header>
                                                            <Accordion.Body>
                                                                <InputGroup className="mb-4">
                                                                    <InputGroup.Text>
                                                                        Array {arrayIndex} offset
                                                                    </InputGroup.Text>
                                                                    <Form.Control
                                                                        type="number"
                                                                        placeholder="LED Pulser Offset"
                                                                        aria-label="LED Pulser Offset"
                                                                        value={array.LEDPulserOffset}
                                                                        onChange={(e) => handleOffsetChange(blockIndex, moduleIndex, arrayIndex, e.target.value)}
                                                                        min="0"
                                                                        max="4095"
                                                                        required
                                                                        isInvalid={validated && (array.LEDPulserOffset < 0 || array.LEDPulserOffset > 4095)}
                                                                    />
                                                                    <Form.Control.Feedback type="invalid">
                                                                        Please enter a value between 0 and 4095.
                                                                    </Form.Control.Feedback>
                                                                    <Button
                                                                        variant={buttonStatus[`offset-${blockIndex}-${moduleIndex}-${arrayIndex}`] === 'success' ? "success" : buttonStatus[`offset-${blockIndex}-${moduleIndex}-${arrayIndex}`] === 'error' ? "danger" : "primary"}
                                                                        type="submit"
                                                                        id="led-pulser-offset-submit-btn"
                                                                        disabled={buttonStatus[`offset-${blockIndex}-${moduleIndex}-${arrayIndex}`] === 'waiting'}
                                                                        onClick={() => handleSetLEDPulserOffset(blockIndex, moduleIndex, arrayIndex)}
                                                                    >
                                                                        {buttonStatus[`offset-${blockIndex}-${moduleIndex}-${arrayIndex}`] === 'waiting' ? (
                                                                            <>
                                                                                Applying offset...
                                                                                <Spinner animation="border" role="status" size="sm" className="ms-2">
                                                                                    <span className="visually-hidden">Loading...</span>
                                                                                </Spinner>
                                                                            </>
                                                                        ) : (
                                                                            buttonStatus[`offset-${blockIndex}-${moduleIndex}-${arrayIndex}`] === 'success' ? "Offset applied" : buttonStatus[`offset-${blockIndex}-${moduleIndex}-${arrayIndex}`] === 'error' ? "Error applying offset" : "Apply offset"
                                                                        )}
                                                                    </Button>
                                                                </InputGroup>
                                                                {array.SiPMs.map((sipm, sipmIndex) => (
                                                                    <InputGroup className="mb-1" key={sipmIndex}>
                                                                        <InputGroup.Text>
                                                                            SiPM {sipmIndex} LED
                                                                        </InputGroup.Text>
                                                                        <Form.Control
                                                                            type="number"
                                                                            placeholder="Pulser readout interval in seconds"
                                                                            aria-label="Readout interval"
                                                                            aria-describedby="pulser-submit-btn"
                                                                            value={sipm.PulserValue}
                                                                            onChange={(e) => handleLineChange(blockIndex, moduleIndex, arrayIndex, sipmIndex, e.target.value)}
                                                                            name="TotalSeconds"
                                                                            min="0"
                                                                            max="4095"
                                                                            required
                                                                            isInvalid={validated && (sipm.PulserValue < 0 || sipm.PulserValue > 4095)}
                                                                        />
                                                                        <Form.Control.Feedback type="invalid">
                                                                            Please enter a value between 0 and 4095.
                                                                        </Form.Control.Feedback>
                                                                        <Button
                                                                            variant={buttonStatus[`pulser-${blockIndex}-${moduleIndex}-${arrayIndex}-${sipmIndex}`] === 'success' ? "success" : buttonStatus[`pulser-${blockIndex}-${moduleIndex}-${arrayIndex}-${sipmIndex}`] === 'error' ? "danger" : "primary"}
                                                                            type="button"
                                                                            id="pulser-submit-btn"
                                                                            disabled={buttonStatus[`pulser-${blockIndex}-${moduleIndex}-${arrayIndex}-${sipmIndex}`] === 'waiting'}
                                                                            onClick={() => handleSetPulserValue(blockIndex, moduleIndex, arrayIndex, sipmIndex)}
                                                                        >
                                                                            {buttonStatus[`pulser-${blockIndex}-${moduleIndex}-${arrayIndex}-${sipmIndex}`] === 'waiting' ? (
                                                                                <>
                                                                                    Applying pulser...
                                                                                    <Spinner animation="border" role="status" size="sm" className="ms-2">
                                                                                        <span className="visually-hidden">Loading...</span>
                                                                                    </Spinner>
                                                                                </>
                                                                            ) : (
                                                                                buttonStatus[`pulser-${blockIndex}-${moduleIndex}-${arrayIndex}-${sipmIndex}`] === 'success' ? "Pulser applied" : buttonStatus[`pulser-${blockIndex}-${moduleIndex}-${arrayIndex}-${sipmIndex}`] === 'error' ? "Error applying pulser" : "Apply pulser"
                                                                            )}
                                                                        </Button>
                                                                    </InputGroup>
                                                                ))}
                                                            </Accordion.Body>
                                                        </Accordion.Item>
                                                    </Accordion>
                                                ))}
                                            </Accordion.Body>
                                        </Accordion.Item>
                                    </Accordion>
                                ))}
                                <div className="mb-3"></div>
                            </Accordion.Body>
                        </Accordion.Item>
                    ))}
                </Accordion>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="primary" onClick={handleDownloadLEDValuesClick}>
                    Export LED values as JSON
                </Button>
                <Button variant="primary" onClick={handleDownloadArrayOffsetsClick}>
                    Export Array offsets as JSON
                </Button>
                <Button variant="secondary" onClick={handleClose}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
};

export default PulserValuesModal;

