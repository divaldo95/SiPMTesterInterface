import React, { useState, useRef } from 'react';
import { Modal, Carousel, Button, Form } from 'react-bootstrap';
import { BarcodeScanner } from 'react-barcode-scanner';
import { useMeasurement } from '../context/MeasurementContext';
import 'bootstrap/dist/css/bootstrap.min.css';

function MeasurementWizard({ show, onHide }) {
    const { measurementData, updateBarcode } = useMeasurement();
    const [scanning, setScanning] = useState(false);
    const barcodeRefs = useRef([]);

    const handleScan = (blockIndex, moduleIndex, arrayIndex, data) => {
        if (data) {
            updateBarcode(blockIndex, moduleIndex, arrayIndex, data);
            setScanning(false);
        }
    };

    const handleError = (err) => {
        console.error(err);
    };

    const renderArrays = () => {
        const items = [];
        measurementData.Blocks.forEach((block, blockIndex) => {
            block.Modules.forEach((module, moduleIndex) => {
                module.Arrays.forEach((array, arrayIndex) => {
                    items.push(
                        <Carousel.Item key={`${blockIndex}-${moduleIndex}-${arrayIndex}`}>
                            <div className="d-flex flex-column align-items-center">
                                <h3>Scan Barcode for Block {blockIndex + 1}, Module {moduleIndex + 1}, Array {arrayIndex + 1}</h3>
                                <Form className="w-50">
                                    <Form.Group className="mb-3">
                                        <Form.Label>Barcode</Form.Label>
                                        <Form.Control
                                            type="text"
                                            value={array.Barcode}
                                            onChange={(e) => updateBarcode(blockIndex, moduleIndex, arrayIndex, e.target.value)}
                                            ref={(el) => (barcodeRefs.current[`${blockIndex}-${moduleIndex}-${arrayIndex}`] = el)}
                                        />
                                    </Form.Group>
                                    <Button variant="secondary" onClick={() => setScanning(!scanning)}>
                                        {scanning ? 'Stop Scanning' : 'Start Scanning'}
                                    </Button>
                                </Form>
                                {scanning && (
                                    <BarcodeScanner
                                        onUpdate={(err, result) => {
                                            if (result) handleScan(blockIndex, moduleIndex, arrayIndex, result.text);
                                            if (err) handleError(err);
                                        }}
                                    />
                                )}
                            </div>
                        </Carousel.Item>
                    );
                });
            });
        });
        return items;
    };

    if (!show) {
        return null;
    }

    return (
        <Modal show={show} onHide={onHide} fullscreen>
            <Modal.Header closeButton>
                <Modal.Title>Measurement Wizard</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Carousel controls={true} indicators={false} interval={null}>
                    {renderArrays()}
                </Carousel>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={onHide}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
}

export default MeasurementWizard;
