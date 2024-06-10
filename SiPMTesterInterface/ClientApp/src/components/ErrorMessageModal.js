import React, { useState } from 'react';
import { Modal, Button, Row, Col, Alert } from 'react-bootstrap';
import { ResponseButtons } from '../enums/ResponseButtons';

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
            default:
                return null;
        }
    };

    return (
        <Modal show={show} onHide={handleClose} backdrop="static" centered size="lg">
            <Modal.Header closeButton>
                <Modal.Title>
                    {error.Sender}
                </Modal.Title>
            </Modal.Header>
            <Modal.Body className="text-center">
                <Alert key="danger" variant="danger">
                    {error.Message}
                </Alert>
            </Modal.Body>
            <Modal.Footer>
                {renderButtons(error.ValidInteractionButtons)}
            </Modal.Footer>
        </Modal>
    );
};

export default ErrorMessageModal;
