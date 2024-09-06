import { useContext, useState } from 'react';
import { MeasurementContext } from '../context/MeasurementContext';
import { Modal, Button, Table } from 'react-bootstrap';
import MeasurementStateService from '../services/MeasurementStateService';

function ExportMeasurementModal(props) {
    const { showModal, closeModal } = props;

    const { measurementStates } = useContext(MeasurementContext);

    const filterSiPMs = (data) => {
        const filteredSiPMs = [];

        data.Blocks.forEach((block, blockIndex) => {
            block.Modules.forEach((module, moduleIndex) => {
                module.Arrays.forEach((array, arrayIndex) => {
                    array.SiPMs.forEach((sipm, sipmIndex) => {
                        const checks = sipm.Checks;
                        const isSelected = checks.SelectedForMeasurement;
                        const allDone = [
                            checks.RForwardDone,
                            checks.IDarkDone,
                            checks.IVDone
                        ].every(Boolean);
                        const anyOk = [
                            checks.RForwardOK,
                            checks.IDarkOK,
                            checks.IVTemperatureOK,
                            checks.IVMeasurementOK,
                            checks.IVVoltageCheckOK,
                            checks.IVCurrentCheckOK,
                            checks.IVVbrOK
                        ].some(Boolean);

                        if (isSelected && allDone && !anyOk) {
                            filteredSiPMs.push({
                                BlockIndex: blockIndex,
                                ModuleIndex: moduleIndex,
                                ArrayIndex: arrayIndex,
                                SiPMIndex: sipmIndex,
                                SiPM: sipm
                            });
                        }
                    });
                });
            });
        });

        return filteredSiPMs;
    };

    const filteredSiPMs = filterSiPMs(measurementStates);

    return (
        <Modal scrollable show={showModal} onHide={closeModal} centered size="lg" fullscreen={false}>
            <Modal.Header closeButton>
                <Modal.Title>{`Export measurement`}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <div className="text-center d-block d-inline-block">
                    <div className="row">
                        <div className="col">
                            <h3 className="mb-3">Failed measurements</h3>
                            <Table striped bordered hover>
                                <thead>
                                    <tr>
                                        <th>BlockIndex</th>
                                        <th>ModuleIndex</th>
                                        <th>ArrayIndex</th>
                                        <th>SiPMIndex</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {filteredSiPMs.map((item, index) => (
                                        <tr key={index}>
                                            <td>{item.BlockIndex}</td>
                                            <td>{item.ModuleIndex}</td>
                                            <td>{item.ArrayIndex}</td>
                                            <td>{item.SiPMIndex}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
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

export default ExportMeasurementModal;
