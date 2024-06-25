import React, { useEffect, useState } from 'react';
import MeasurementStateService from '../services/MeasurementStateService';
import { Modal, Button, Accordion, Card, Badge } from 'react-bootstrap';

const PulserValuesModalOld = ({ show, handleClose }) => {
    const [sensorData, setSensorData] = useState([]);
    const [buttonStatus, setButtonStatus] = useState([]); // State to store button statuses


    const getPulserLEDValues = async () => {
        try {
            const data = await MeasurementStateService.getPulserLEDValues();
            setSensorData(data);
            setButtonStatus(Array(data.length).fill(null)); // Initialize button statuses
            console.log(data);
        } catch (error) {
            console.log(error);
        }
    };

    useEffect(() => {
        getPulserLEDValues();
    }, []);

    const exportToJson = () => {
        const dataStr = JSON.stringify(sensorData, null, 2);
        const blob = new Blob([dataStr], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'sensorData.json';
        link.click();
        URL.revokeObjectURL(url);
    };

    const renderAccordionItems = () => {
        const groupedData = {};

        // Group data by Block, Module, and Array
        sensorData.forEach(item => {
            const { Block, Module, Array, SiPM } = item.SiPM;
            if (!groupedData[Block]) groupedData[Block] = {};
            if (!groupedData[Block][Module]) groupedData[Block][Module] = {};
            if (!groupedData[Block][Module][0]) groupedData[Block][Module][0] = [];

            groupedData[Block][Module][0].push(item);
        });

        return Object.keys(groupedData).map(block => (
            <Card key={`block-${block}`}>
                <Accordion.Toggle as={Card.Header} eventKey={`block-${block}`}>
                    Block {block}
                </Accordion.Toggle>
                <Accordion.Collapse eventKey={`block-${block}`}>
                    <Card.Body>
                        {Object.keys(groupedData[block]).map(module => (
                            <Accordion key={`module-${block}-${module}`} className="my-2">
                                <Card>
                                    <Accordion.Toggle as={Card.Header} eventKey={`module-${block}-${module}`}>
                                        Module {module}
                                    </Accordion.Toggle>
                                    <Accordion.Collapse eventKey={`module-${block}-${module}`}>
                                        <Card.Body>
                                            {Object.keys(groupedData[block][module]).map(array => (
                                                <Accordion key={`array-${block}-${module}-${array}`} className="my-2">
                                                    <Card>
                                                        <Accordion.Toggle as={Card.Header} eventKey={`array-${block}-${module}-${array}`}>
                                                            Array {array}
                                                        </Accordion.Toggle>
                                                        <Accordion.Collapse eventKey={`array-${block}-${module}-${array}`}>
                                                            <Card.Body>
                                                                <ul>
                                                                    {groupedData[block][module][array].map((item, index) => (
                                                                        <li key={index}>
                                                                            SiPM {item.SiPM.SiPM}: PulserValue {item.PulserValue}
                                                                        </li>
                                                                    ))}
                                                                </ul>
                                                            </Card.Body>
                                                        </Accordion.Collapse>
                                                    </Card>
                                                </Accordion>
                                            ))}
                                        </Card.Body>
                                    </Accordion.Collapse>
                                </Card>
                            </Accordion>
                        ))}
                    </Card.Body>
                </Accordion.Collapse>
            </Card>
        ));
    };

    const handleLineChange = (index, value) => {
        const updatedList = [...sensorData];
        updatedList[index].PulserValue = value;
        setSensorData(updatedList);
    };

    const handleSetPulserValue = (e, index) => {
        const data = sensorData[index];
        try {
            MeasurementStateService.setPulserLEDValue(data.SiPM.Block, data.SiPM.Module, data.SiPM.Array, data.SiPM.SiPM, data.PulserValue);
            console.log(data);
            updateButtonStatus(index, 'success');
        } catch (error) {
            console.log(error);
            updateButtonStatus(index, 'error');
        }
        // Set a timeout to reset the button status after 5 seconds
        setTimeout(() => {
            updateButtonStatus(index, null);
        }, 5000);
    };

    const updateButtonStatus = (index, status) => {
        const newStatus = [...buttonStatus];
        newStatus[index] = status;
        setButtonStatus(newStatus);
    };


    return (
        <Modal scrollable show={show} onHide={handleClose} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Sensor Data</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <ul className="list-group">
                    {sensorData.map((data, index) => (
                        <li key={index} className="input-group-sm mb-1 d-flex justify-content-between align-items-center">
                            <Badge className="me-2">B: {data.SiPM.Block}</Badge>
                            <Badge className="me-2">M: {data.SiPM.Module}</Badge>
                            <Badge className="me-2">A: {data.SiPM.Array}</Badge>
                            <Badge className="me-2">S: {data.SiPM.SiPM}</Badge>
                            <input
                                type="number"
                                min="1"
                                step="1"
                                max="4095"
                                className="form-control"
                                value={data.PulserValue}
                                onChange={(e) => handleLineChange(index, e.target.value)}
                            />
                            <button
                                className={`btn ${buttonStatus[index] === 'success' ? 'btn-success' : buttonStatus[index] === 'error' ? 'btn-danger' : 'btn-primary'}`}
                                onClick={(e) => handleSetPulserValue(e, index)}
                            >
                                <i className="bi bi-check-square"></i>
                            </button>
                        </li>
                    ))}
                </ul>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="primary" onClick={exportToJson}>
                    Export as JSON
                </Button>
                <Button variant="secondary" onClick={handleClose}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
};

export default PulserValuesModalOld;

