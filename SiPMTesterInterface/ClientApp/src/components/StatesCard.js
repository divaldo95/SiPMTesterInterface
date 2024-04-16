import { useContext, useEffect } from 'react';
import { getConnectionStatusBtnClasses } from '../enums/ConnectionStateEnum';

function StatesCard(props) {
    const { className, MeasurementType, ConnectionState, MeasurementState } = props;

    const connectionClasses = getConnectionStatusBtnClasses(ConnectionState);
    const measurementClasses = getConnectionStatusBtnClasses(MeasurementState);

    useEffect(() => {
        //console.log(ConnectionState);
        //console.log(MeasurementState);
    }, []);

    return (
        <>
            <div className={`card ${className}`}>
                <h5 className="card-header">{ MeasurementType } states</h5>
                <div className="card-body d-flex align-items-center justify-content-center">
                    <div className="container text-center">
                        <div className="btn-group" role="group" aria-label="sensor block with settings">
                            <button disabled={false} onClick={(e) => { e.preventDefault(); }} className={`btn ${connectionClasses.buttonColor} ${connectionClasses.textColor}`}>
                                Connection: <i className={`ms-1 bi ${connectionClasses.icon}`}></i>
                            </button>
                            <button disabled={false} onClick={(e) => { e.preventDefault(); }} className={`btn ${measurementClasses.buttonColor} ${measurementClasses.textColor}`}>
                                Measurement: <i className={`ms-1 bi ${measurementClasses.icon}`}></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}

export default StatesCard;