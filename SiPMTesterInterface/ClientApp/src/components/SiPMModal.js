import React from 'react';
import { Modal, Button } from 'react-bootstrap';
import RootHistogram from './RootHistogram';

class SiPMModal extends React.Component {
    render() {
        const { showModal, closeModal, arrayNumber, sipmNumber } = this.props;

        return (
            <Modal show={showModal} onHide={closeModal} centered size="lg" fullscreen={false}>
                <Modal.Header closeButton>
                    <Modal.Title>SiPM Box Details</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <p>You clicked on SiPM {sipmNumber} from array {arrayNumber}.</p>
                    <RootHistogram>
                    </RootHistogram>
                    {/* Add more details or components here if needed */}
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={closeModal}>
                        Close
                    </Button>
                </Modal.Footer>
            </Modal>
        );
    }
}

export default SiPMModal;
