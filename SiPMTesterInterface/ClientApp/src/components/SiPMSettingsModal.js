import React from 'react';
import { Modal, Button } from 'react-bootstrap';
import { MeasurementProvider } from '../context/MeasurementContext';
import ModeSelectButtonGroup from './ModeSelectButtonGroup';
import VoltageListComponent from './VoltageListComponent';

function SiPMSettingsModal(props) {
    const { showModal, closeModal, BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex } = props;

    return (
        <Modal show={showModal} onHide={closeModal} centered size="lg" fullscreen={false}>
            <Modal.Header closeButton>
                <Modal.Title>{`SiPM Settings (Block: ${BlockIndex} Module: ${ModuleIndex} Array: ${ArrayIndex} SiPM: ${SiPMIndex})`}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <div className="text-center d-block d-inline-block">
                    <div className="row justify-content-center">
                        <div className="col mb-3">
                            <ModeSelectButtonGroup BlockIndex={BlockIndex} ModuleIndex={ModuleIndex} ArrayIndex={ArrayIndex} SiPMIndex={SiPMIndex}> </ModeSelectButtonGroup>
                        </div>
                    </div>
                    <div className="row">
                        <div className="col">
                            <VoltageListComponent className="" MeasurementMode="IV" BlockIndex={BlockIndex} ModuleIndex={ModuleIndex} ArrayIndex={ArrayIndex} SiPMIndex={SiPMIndex}></VoltageListComponent>
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

export default SiPMSettingsModal;