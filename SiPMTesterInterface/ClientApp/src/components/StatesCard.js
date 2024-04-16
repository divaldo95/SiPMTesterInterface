import { useContext, useEffect } from 'react';
import { getConnectionStatusBtnClasses, ConnectionStatusString } from '../enums/ConnectionStateEnum';
import { MeasurementStatusString, getMeasurementStatusBtnClasses } from '../enums/StatusEnum';
import OverlayTrigger from 'react-bootstrap/OverlayTrigger';
import Tooltip from 'react-bootstrap/Tooltip';

function StatesCard(props) {
    const { className, MeasurementType, ConnectionState, MeasurementState } = props;

    const connectionClasses = getConnectionStatusBtnClasses(ConnectionState);
    const measurementClasses = getMeasurementStatusBtnClasses(MeasurementState);

    useEffect(() => {
        //console.log(ConnectionState);
        //console.log(MeasurementState);
    }, []);

    const renderConnectionTooltip = (props) => (
        <Tooltip id="button-tooltip" {...props}>
            { ConnectionStatusString(ConnectionState) }
        </Tooltip>
    );

    const renderMeasurementTooltip = (props) => (
        <Tooltip id="button-tooltip" {...props}>
            {MeasurementStatusString(MeasurementState)}
        </Tooltip>
    );

    return (
        <>
            <div className={`card ${className}`}>
                <h5 className="card-header">{ MeasurementType } states</h5>
                <div className="card-body d-flex align-items-center justify-content-center">
                    <div className="container text-center">
                        <div className="btn-group" role="group" aria-label="sensor block with settings">
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
                                <button disabled={false} onClick={(e) => { e.preventDefault(); }} className={`btn ${measurementClasses.buttonColor} ${measurementClasses.textColor}`}>
                                    Measurement: <i className={`ms-1 bi ${measurementClasses.icon}`}></i>
                                </button>
                            </OverlayTrigger>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}

export default StatesCard;