import React, { useContext, useEffect } from 'react';
import { Modal, Button, Table, Badge, Col, Row, OverlayTrigger, Tooltip } from 'react-bootstrap';
import { LogContext } from '../context/LogContext';
import { getResponseButtonString } from '../enums/ResponseButtons';
import { getLogMessageTypeMarkDetails, LogMessageType } from '../enums/LogMessageTypeEnum';

const LogModal = ({ show, handleClose }) => {
    const { logs, fetchLogs } = useContext(LogContext);

    useEffect(() => {
        if (show) {
            //fetchLogs();
            console.log("fetched errors");
        }
    }, [show]);

    const renderLogType = (logType) => {
        let properties = getLogMessageTypeMarkDetails(logType);

        return (
            <OverlayTrigger
              placement="right"
                delay={{ show: 250, hide: 400 }}
                overlay={<Tooltip id="button-tooltip">{properties.Tooltip}</Tooltip>}
            >
                <i className={`bi ${properties.Mark} ${properties.Color}`}></i>
            </OverlayTrigger>
            
        );
    };

    const renderInteractionBadge = (interactionNeeded) => {
        let color = "success";
        let text = "No";

        if (interactionNeeded) {
            color = "danger";
            text = "Yes";
        }

        return (
            <Badge bg={color}>{text}</Badge>
        );
    };

    const renderResolvedBadge = (resolved) => {
        let color = "success";
        let text = "Yes";

        if (!resolved) {
            color = "danger";
            text = "No";
        }

        return (
            <Badge bg={color}>{text}</Badge>
        );
    };

    return (
        <Modal show={show} onHide={handleClose} size="xl" scrollable centered aria-labelledby="log-modal-title">
            <Modal.Header closeButton>
                <Modal.Title id="log-modal-title">
                    Log Messages
                </Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Table striped bordered hover>
                    <thead>
                        <tr className="text-center">
                            <th>Type</th>
                            <th>Sender</th>
                            <th>Message</th>
                            <th>User Response</th>
                            <th>Timestamp</th>
                            <th>Needs Interaction</th>
                            <th>Resolved</th>
                        </tr>
                    </thead>
                    <tbody className="text-center">
                        {logs.map((log, index) => (
                            <tr key={index}>
                                <td>{renderLogType(log.MessageType)}</td>
                                <td>{log.Sender}</td>
                                <td>{log.Message}</td>
                                <td><Badge bg="primary">{getResponseButtonString(log.UserResponse)}</Badge></td>
                                <td>{new Date(log.Timestamp * 1000).toLocaleDateString()} {new Date(log.Timestamp * 1000).toTimeString().split(' ')[0]}</td>
                                <td>{renderInteractionBadge(log.NeedsInteraction)}</td>
                                <td>{renderResolvedBadge(log.Resolved)}</td>
                            </tr>
                        ))}
                    </tbody>
                </Table>
            </Modal.Body>
            <Modal.Footer>
                <Row>
                    <Col>
                        <Button variant="primary" onClick={fetchLogs}>
                            Refresh
                        </Button>
                    </Col>
                    <Col>
                        <Button variant="secondary" onClick={handleClose}>
                            Close
                        </Button>
                    </Col>
                </Row>
                
            </Modal.Footer>
        </Modal>
    );
};

export default LogModal;
