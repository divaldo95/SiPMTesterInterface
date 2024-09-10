import React, { useState } from 'react';
import { Modal, Button, Row, Col, Table } from 'react-bootstrap';
import { ResponseButtons } from '../enums/ResponseButtons';
import { getLogMessageTypeMarkDetails } from '../enums/LogMessageTypeEnum'

const ExportMessageModal = ({ show, handleClose, skippedList }) => {
    if (!show) {
        return null;
    }

    return (
        <Modal show={show} onHide={handleClose} backdrop="static" centered size="lg">
            <Modal.Header closeButton>
                <Modal.Title className="w-100">
                    <div className="d-flex align-items-center justify-content-between">
                        <div className="flex-grow-1 text-truncate">
                            Skipped measurements by export
                        </div>
                    </div>
                </Modal.Title>
            </Modal.Header>
            <Modal.Body className="text-center">
                <div className="mb-3">
                    There were some issues exporting the following SiPM's data. This can be because measurements for these SiPMs are not available (not measured).
                    Other reason can be permission issues on the file system. Check those and try to export them again if necessary or you can ignore this message.
                </div>
                <Table responsive striped bordered hover size="sm" className="h-100">
                    <thead>
                        <tr>
                            <th>BlockIndex</th>
                            <th>ModuleIndex</th>
                            <th>ArrayIndex</th>
                            <th>SiPMIndex</th>
                        </tr>
                    </thead>
                    <tbody>
                        {skippedList.map((item, index) => (
                            <tr key={index}>
                                <td>{item.Block}</td>
                                <td>{item.Module}</td>
                                <td>{item.Array}</td>
                                <td>{item.SiPM}</td>
                            </tr>
                        ))}
                    </tbody>
                </Table>
            </Modal.Body>
            <Modal.Footer>
                <Button onClick={handleClose} key="OK" variant="primary">OK</Button>;
            </Modal.Footer>
        </Modal>
    );
};

export default ExportMessageModal;
