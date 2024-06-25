import React, { useState } from 'react';
import { Modal, Button, Row, Col, Alert, OverlayTrigger, Tooltip } from 'react-bootstrap';
import { ResponseButtons } from '../enums/ResponseButtons';
import { getLogMessageTypeMarkDetails } from '../enums/LogMessageTypeEnum'

const ErrorMessageModal = ({ show, handleClose, error, handleButtonClick }) => {
    if (!error) return null;

    const renderOKButton = () => {
        return <Button onClick={() => handleButtonClick(error.ID, ResponseButtons.OK)} key="OK" variant="primary">OK</Button>;
    }

    const renderCancelButton = () => {
        return <Button onClick={() => handleButtonClick(error.ID, ResponseButtons.Cancel)} key="Cancel" variant="secondary">Cancel</Button>;
    }

    const renderStopButton = () => {
        return <Button onClick={() => handleButtonClick(error.ID, ResponseButtons.Stop)} key="Stop" variant="danger">Stop</Button>;
    }

    const renderRetryButton = () => {
        return <Button onClick={() => handleButtonClick(error.ID, ResponseButtons.Retry)} key="Retry" variant="warning">Retry</Button>;
    }

    const renderContinueButton = () => {
        return <Button onClick={() => handleButtonClick(error.ID, ResponseButtons.Continue)} key="Continue" variant="success">Continue</Button>;
    }

    const renderYesButton = () => {
        return <Button onClick={() => handleButtonClick(error.ID, ResponseButtons.Yes)} key="Yes" variant="success">Yes</Button>;
    }

    const renderNoButton = () => {
        return <Button onClick={() => handleButtonClick(error.ID, ResponseButtons.No)} key="No" variant="danger">No</Button>;
    }

    const renderButtons = (validInteractionButtons) => {
        switch (validInteractionButtons) {
            case ResponseButtons.CancelOK:
                return (
                    <Row>
                        <Col>
                            {renderCancelButton()}
                        </Col>
                        <Col>
                            {renderOKButton()}
                        </Col>
                    </Row>
                );
            case ResponseButtons.YesNo:
                return (
                    <Row>
                        <Col>
                            {renderNoButton()}
                        </Col>
                        <Col>
                            {renderYesButton()}
                        </Col>
                    </Row>
                );
            case ResponseButtons.StopRetryContinue:
                return (
                    <Row>
                        <Col>
                            {renderStopButton()}
                        </Col>
                        <Col>
                            {renderRetryButton()}
                        </Col>
                        <Col>
                            {renderContinueButton()}
                        </Col>
                    </Row>
                );
            case ResponseButtons.Cancel:
                return (
                    <Row>
                        <Col>
                            {renderCancelButton()}
                        </Col>
                    </Row>
                );
            case ResponseButtons.OK:
                return (
                    <Row>
                        <Col>
                            {renderOKButton()}
                        </Col>
                    </Row>
                );
            case ResponseButtons.Stop:
                return (
                    <Row>
                        <Col>
                            {renderStopButton()}
                        </Col>
                    </Row>
                );
            case ResponseButtons.Retry:
                return (
                    <Row>
                        <Col>
                            {renderRetryButton()}
                        </Col>
                    </Row>
                );
            case ResponseButtons.Continue:
                return (
                    <Row>
                        <Col>
                            {renderContinueButton()}
                        </Col>
                    </Row>
                );
            case ResponseButtons.Yes:
                return (
                    <Row>
                        <Col>
                            {renderYesButton()}
                        </Col>
                    </Row>
                );
            case ResponseButtons.No:
                return (
                    <Row>
                        <Col>
                            {renderNoButton()}
                        </Col>
                    </Row>
                );
            default:
                return null;
        }
    };

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

    return (
        <Modal show={show} onHide={handleClose} backdrop="static" centered size="lg">
            <Modal.Header closeButton>
                <Modal.Title className="w-100">
                    <div className="d-flex align-items-center justify-content-between">
                        <div className="me-2">
                            {renderLogType(error.MessageType)}
                        </div>
                        <div className="flex-grow-1 text-truncate">
                            {error.Sender}
                        </div>
                    </div>
                </Modal.Title>
            </Modal.Header>
            <Modal.Body className="text-center">
                {error.Message}
            </Modal.Body>
            <Modal.Footer>
                {renderButtons(error.ValidInteractionButtons)}
            </Modal.Footer>
        </Modal>
    );
};

export default ErrorMessageModal;
