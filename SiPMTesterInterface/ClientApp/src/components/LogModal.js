import React, { useContext, useEffect, useState } from 'react';
import { Modal, Button, Table, Badge, Col, Row, OverlayTrigger, Tooltip } from 'react-bootstrap';
import { LogContext } from '../context/LogContext';
import { MeasurementContext } from '../context/MeasurementContext';
import { getResponseButtonString } from '../enums/ResponseButtons';
import { getLogMessageTypeMarkDetails, LogMessageType } from '../enums/LogMessageTypeEnum';
import MeasurementStateService from '../services/MeasurementStateService';

const LogModal = ({ show, handleClose }) => {
    const { activeSiPMs } = useContext(MeasurementContext);
    const { logs, fetchLogs } = useContext(LogContext);
    const [canForceRestartRequested, setCanForceRestartRequested] = useState(false);
    const [countdown, setCountdown] = useState(30); // Set initial countdown to 30 seconds

    const requestForceRestart = async () => {
        setCanForceRestartRequested(false); //disable button immediately
        MeasurementStateService.forceRestartMeasurement()
            .then((resp) => {
                console.log("Force restart requested successfully");
                setCountdown(30);
            }).catch((error) => {
                console.error('Error in force restart:', error); // Catch and handle error
            });
    };

    const refreshForceRestartInformation = async () => {
        try {
            await MeasurementStateService.getForceRestartInformation()
                .then((resp) => {
                    setCanForceRestartRequested(resp.CanRequest);
                    let waitingTime = resp.WaitingTime - resp.ElapsedTime;
                    if (waitingTime > 0) {
                        setCountdown(waitingTime);
                    }
                    console.log(resp);
                })
        } catch (error) {
            console.log(error);
        }
    }

    useEffect(() => {
        if (show) {
            refreshForceRestartInformation();
        }
    }, [show]);

    // Function to reset countdown when activeSiPMs changes
    useEffect(() => {
        setCountdown(30); // Reset countdown to 30 when activeSiPMs changes
        setCanForceRestartRequested(false); // Reset the force restart flag when the countdown is reset
    }, [activeSiPMs]);

    useEffect(() => {
        if (countdown > 0) {
            const interval = setInterval(() => {
                setCountdown(prevCountdown => prevCountdown - 1);
            }, 1000);
            return () => clearInterval(interval); // Clear the interval when the component unmounts or countdown changes
        } else {
            // Trigger the flag when countdown reaches 0
            setCanForceRestartRequested(true);
        }
    }, [countdown]);

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

    if (!show) {
        return null;
    }

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
                <Row className="w-100">
                    {/* Left-aligned buttons */}
                    <Col className="d-flex justify-content-start">
                        <OverlayTrigger
                            placement="top"
                            delay={{ show: 250, hide: 400 }}
                            overlay={<Tooltip id="button-tooltip">Refresh force restart state</Tooltip>}
                        >
                            <Button variant="success" onClick={refreshForceRestartInformation}>
                                <i className="bi bi-arrow-repeat"></i>
                            </Button>
                        </OverlayTrigger>
                        <OverlayTrigger
                            placement="top"
                            delay={{ show: 250, hide: 400 }}
                            overlay={<Tooltip id="button-tooltip">Request force restart of current measurement</Tooltip>}
                        >
                            <Button disabled={!canForceRestartRequested} variant="warning" onClick={requestForceRestart} className="ms-2">
                                {countdown > 0 ? `Waiting... ${countdown}s` : <i className="bi bi-bootstrap-reboot"></i>}
                            </Button>
                        </OverlayTrigger>
                        
                    </Col>

                    {/* Right-aligned buttons */}
                    <Col className="d-flex justify-content-end">
                        <Button variant="primary" onClick={fetchLogs}>
                            Refresh
                        </Button>
                        <Button variant="secondary" onClick={handleClose} className="ms-2">
                            Close
                        </Button>
                    </Col>
                </Row>
            </Modal.Footer>
        </Modal>
    );
};

export default LogModal;
