import { useContext, useEffect, useState } from 'react';
import { getConnectionStatusBtnClasses, ConnectionStatusString, SerialConnectionStateEnum } from '../enums/SerialStatusEnum';
import OverlayTrigger from 'react-bootstrap/OverlayTrigger';
import Tooltip from 'react-bootstrap/Tooltip';
import PulserModalComponent from './PulserModalComponent';

function SerialStateCard(props) {
    const { className, SerialInstrumentName, ConnectionState, MeasurementState } = props;
    const [showModal, setShowModal] = useState(false);

    const openModal = () => {
        if (ConnectionState === SerialConnectionStateEnum.Connected || ConnectionState === SerialConnectionStateEnum.Timeout) {
            setShowModal(true);
        }
    };

    const closeModal = () => {
        setShowModal(false);
    };

    const connectionClasses = getConnectionStatusBtnClasses(ConnectionState);

    useEffect(() => {
    }, []);

    const renderConnectionTooltip = (props) => (
        <Tooltip id="button-tooltip" {...props}>
            { ConnectionStatusString(ConnectionState) }
        </Tooltip>
    );

    const renderMeasurementTooltip = (props) => (
        <Tooltip id="button-tooltip" {...props}>
            Open details
        </Tooltip>
    );

    return (
        <>
            <div className={`card ${className}`}>
                <h5 className="card-header">{SerialInstrumentName } state</h5>
                <div className="card-body d-flex align-items-center justify-content-center">
                    <div className="container text-center">
                        <div className="btn-group" role="group" aria-label="serial instrument with settings">
                            <OverlayTrigger
                                placement="bottom"
                                delay={{ show: 250, hide: 400 }}
                                overlay={renderConnectionTooltip}
                            >
                                <button disabled={false} onClick={(e) => { e.preventDefault(); }} className={`btn ${connectionClasses.buttonColor} ${connectionClasses.textColor}`}>
                                    Connection: <i className={`ms-1 bi ${connectionClasses.icon}`}></i>
                                </button>
                            </OverlayTrigger>
                            <OverlayTrigger
                                placement="bottom"
                                delay={{ show: 250, hide: 400 }}
                                overlay={renderMeasurementTooltip}
                            >
                                <button disabled={false} onClick={(e) => { e.preventDefault(); openModal(); }} className={`btn ${connectionClasses.buttonColor} ${connectionClasses.textColor}`}>
                                    Details: <i className={`ms-1 bi bi-gear`}></i>
                                </button>
                            </OverlayTrigger>
                        </div>
                    </div>
                </div>
            </div>
            <PulserModalComponent showModal={showModal} closeModal={closeModal}/>
        </>
    );
}

export default SerialStateCard;