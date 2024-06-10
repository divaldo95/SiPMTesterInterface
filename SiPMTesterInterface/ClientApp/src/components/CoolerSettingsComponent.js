import React, { useState, useEffect } from 'react';
import { Accordion, Card, Button, Form, Container, Row, Col, Spinner, FloatingLabel } from 'react-bootstrap';
import MeasurementStateService from '../services/MeasurementStateService';
import 'bootstrap/dist/css/bootstrap.min.css';

function CoolerSettingsComponent() {
    const [isCoolerWaitingUpdate, setIsCoolerWaitingUpdate] = useState(false);
    const [isCoolerUpdateSuccess, setIsCoolerUpdateSuccess] = useState(false);
    const [isCoolerUpdateError, setIsCoolerUpdateError] = useState(false);

    const [blocks, setBlocks] = useState([
        {
            modules: [
                { enabled: false, targetTemperature: 0.0, fanSpeed: 50 },
                { enabled: false, targetTemperature: 0.0, fanSpeed: 50 }
            ]
        },
        {
            modules: [
                { enabled: false, targetTemperature: 0.0, fanSpeed: 50 },
                { enabled: false, targetTemperature: 0.0, fanSpeed: 50 }
            ]
        }
    ]);

    const handleInputChange = (blockIndex, moduleIndex, field, value) => {
        const updatedBlocks = [...blocks];
        updatedBlocks[blockIndex].modules[moduleIndex][field] = value;
        setBlocks(updatedBlocks);
    };

    const handleCoolerSubmit = (e, blockIndex, moduleIndex) => {
        e.preventDefault();

        var data = {
            Block: blockIndex,
            Module: moduleIndex,
            Enabled: blocks[blockIndex].modules[moduleIndex].enabled,
            TargetTemperature: blocks[blockIndex].modules[moduleIndex].targetTemperature,
            FanSpeed: blocks[blockIndex].modules[moduleIndex].fanSpeed
        }

        MeasurementStateService.setCooler(data.Block, data.Module, data.Enabled, data.TargetTemperature, data.FanSpeed)
            .then((resp) => {
                setIsCoolerUpdateSuccess(true);
                setIsCoolerUpdateError(false);
                setIsCoolerWaitingUpdate(false);

                setTimeout(() => {
                    setIsCoolerUpdateSuccess(false);
                    setIsCoolerUpdateError(false);
                }, 3000);
            }).catch((err) => {
                //alert("Pulser not set")
                setIsCoolerUpdateSuccess(false);
                setIsCoolerUpdateError(true);
                setIsCoolerWaitingUpdate(false);

                setTimeout(() => {
                    setIsCoolerUpdateSuccess(false);
                    setIsCoolerUpdateError(false);
                }, 3000);
            });
        console.log("Submitted cooler");
    };

    return (
        <Container>
            <Accordion defaultActiveKey="0">
                {blocks.map((block, blockIndex) => (
                    block.modules.map((module, moduleIndex) => (
                        <Accordion.Item eventKey={blockIndex * 2 + moduleIndex}>
                            <Accordion.Header>Block {blockIndex} | Module {moduleIndex}</Accordion.Header>
                            <Accordion.Body>
                                <Form>
                                    <Row className="mb-3 justify-content-md-center">
                                        <Col>
                                            <FloatingLabel
                                                controlId="floatingTextarea"
                                                label="Target Temperature (°C)"
                                                className="mb-3"
                                            >
                                                <Form.Control
                                                    type="number"
                                                    step="0.1"
                                                    min="-40.0"
                                                    max="40.0"
                                                    value={module.targetTemperature}
                                                    onChange={(e) => handleInputChange(blockIndex, moduleIndex, 'targetTemperature', parseFloat(e.target.value))}
                                                />
                                            </FloatingLabel>
                                        </Col>
                                        <Col>
                                            <FloatingLabel
                                                controlId="floatingTextarea"
                                                label="Fan Speed (%)"
                                                className="mb-3"
                                            >
                                                <Form.Control
                                                    type="number"
                                                    min="0"
                                                    max="100"
                                                    value={module.fanSpeed}
                                                    onChange={(e) => handleInputChange(blockIndex, moduleIndex, 'fanSpeed', parseInt(e.target.value))}
                                                />
                                            </FloatingLabel>
                                        </Col>
                                    </Row>
                                    <Row className="justify-content-center">
                                        <Col className="d-flex justify-content-center">
                                            <Form.Group controlId={`block-${blockIndex}-module-${moduleIndex}-enabled`}>
                                                <Form.Check
                                                    type="switch"
                                                    label="Enabled"
                                                    checked={module.enabled}
                                                    onChange={(e) => handleInputChange(blockIndex, moduleIndex, 'enabled', e.target.checked)}
                                                />
                                            </Form.Group>
                                        </Col>
                                        <Col className="d-flex justify-content-center">
                                            <button
                                                onClick={(e) => handleCoolerSubmit(e, blockIndex, moduleIndex)}
                                                className={`btn ${isCoolerUpdateSuccess ? "btn-success" : isCoolerUpdateError ? "btn-danger" : "btn-primary"}`}
                                                disabled={isCoolerUpdateError || isCoolerUpdateSuccess || isCoolerWaitingUpdate}
                                            >
                                                {isCoolerWaitingUpdate ? (
                                                    <>
                                                        Applying cooler...
                                                        <Spinner animation="border" role="status" size="sm" className="ms-2">
                                                            <span className="visually-hidden">Loading...</span>
                                                        </Spinner>
                                                    </>
                                                ) : (
                                                    isCoolerUpdateSuccess ? "Cooler applied" : isCoolerUpdateError ? "Error applying cooler" : "Apply cooler"
                                                )}
                                            </button>
                                        </Col>
                                    </Row>
                                       
                                </Form>
                            </Accordion.Body>
                        </Accordion.Item>
                    ))
                ))}
            </Accordion>
        </Container>
    );
}

export default CoolerSettingsComponent;
