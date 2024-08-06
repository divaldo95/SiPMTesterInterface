import React, { useContext, useState, useEffect, useRef } from 'react';
import { Modal, Button, InputGroup, Spinner, Form, Col, Row, FloatingLabel } from 'react-bootstrap';
import { MeasurementContext } from '../context/MeasurementContext';
import ArraySettingsComponent from './ArraySettingsComponent';

function ArraySettingsModal(props) {
    const { showModal, closeModal, BlockIndex, ModuleIndex, ArrayIndex, handleBarcodeChange, isBarcodeFetching } = props;

    return (
        <Modal show={showModal} onHide={closeModal} centered size="lg" fullscreen={false}>
            <Modal.Header closeButton>
                <Modal.Title>{`Array Settings (Block: ${BlockIndex} Module: ${ModuleIndex} Array: ${ArrayIndex})`}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <ArraySettingsComponent show={showModal} BlockIndex={BlockIndex} ModuleIndex={ModuleIndex} ArrayIndex={ArrayIndex} handleBarcodeChange={handleBarcodeChange} isBarcodeFetching={isBarcodeFetching} />
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={closeModal}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
}

export default ArraySettingsModal;
