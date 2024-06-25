import React, { useState, useEffect } from 'react';
import { Accordion, Card, Button, Form, Container, Row, Col, Spinner, FloatingLabel, Badge } from 'react-bootstrap';
import MeasurementStateService from '../services/MeasurementStateService';
import { CoolerStateProps, CoolerState } from '../enums/CoolerStateEnum';
import 'bootstrap/dist/css/bootstrap.min.css';

function CoolerSettingsComponent({ coolerData, updateCoolerData }) {
    const [isCoolerWaitingUpdate, setIsCoolerWaitingUpdate] = useState(false);
    const [isCoolerUpdateSuccess, setIsCoolerUpdateSuccess] = useState(false);
    const [isCoolerUpdateError, setIsCoolerUpdateError] = useState(false);

    const handleInputChange = (blockIndex, moduleIndex, field, value) => {
        
        const updatedData = [...coolerData.CoolerSettings];
        const index = blockIndex * coolerData.ModuleNum + moduleIndex;
        updatedData[index][field] = value;
        updateCoolerData(updatedData);
    };

    const handleCoolerSubmit = (e, blockIndex, moduleIndex) => {
        e.preventDefault();

        var data = {
            Block: blockIndex,
            Module: moduleIndex,
            Enabled: coolerData.CoolerSettings[blockIndex * coolerData.ModuleNum + moduleIndex].Enabled,
            TargetTemperature: coolerData.CoolerSettings[blockIndex * coolerData.ModuleNum + moduleIndex].TargetTemperature,
            FanSpeed: coolerData.CoolerSettings[blockIndex * coolerData.ModuleNum + moduleIndex].FanSpeed
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

    const renderBadges = (settings) => {
        let stateProps = CoolerStateProps(settings.State.ActualState);
        return (
            <Col className="">
                <Badge pill bg={stateProps.Color} className="ms-2">{stateProps.StateMessage}</Badge>
                {settings.State.IsTemperatureStable ? <Badge pill bg="success" className="ms-2">Stable</Badge> : <Badge pill bg="primary" className="ms-2">Not stable</Badge>}
                <Badge pill bg="primary" className="ms-2">{settings.State.CoolerTemperature}°C</Badge>
                <Badge pill bg="primary" className="ms-2">{settings.State.PeltierVoltage}V</Badge>
                <Badge pill bg="primary" className="ms-2">{settings.State.PeltierCurrent}A</Badge>
            </Col>
        )
    }

    return (
        <Container>
            <Accordion defaultActiveKey="0">
                {coolerData.CoolerSettings.map((settings, index) => (
                    <Accordion.Item key={index} eventKey={index}>
                        <Accordion.Header>
                            <span>Block {settings.Block} | Module {settings.Module}</span>
                            {renderBadges(settings)}
                        </Accordion.Header>
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
                                                value={settings.TargetTemperature}
                                                onChange={(e) => handleInputChange(settings.Block, settings.Module, 'TargetTemperature', parseFloat(e.target.value))}
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
                                                value={settings.FanSpeed}
                                                onChange={(e) => handleInputChange(settings.Block, settings.Module, 'FanSpeed', parseInt(e.target.value))}
                                            />
                                        </FloatingLabel>
                                    </Col>
                                </Row>
                                <Row className="justify-content-center">
                                    <Col className="d-flex justify-content-center">
                                        <Form.Group controlId={`block-${settings.Block}-module-${settings.Module}-enabled`}>
                                            <Form.Check
                                                type="switch"
                                                label="Enabled"
                                                checked={settings.Enabled}
                                                onChange={(e) => handleInputChange(settings.Block, settings.Module, 'Enabled', e.target.checked)}
                                            />
                                        </Form.Group>
                                    </Col>
                                    <Col className="d-flex justify-content-center">
                                        <button
                                            onClick={(e) => handleCoolerSubmit(e, settings.Block, settings.Module)}
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
                ))}
            </Accordion>
        </Container>
    );
}

export default CoolerSettingsComponent;
