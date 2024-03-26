
import { createContext } from 'react';
import SiPMSensor from './SiPMSensor';
import SiPMArray from './SiPMArray';
import SiPMModule from './SiPMModule';
import SiPMBlock from './SiPMBlock';
import { MeasurementProvider } from '../context/MeasurementContext';

function ButtonsComponent({ buttons, className }) {

    return (

        <>
            <div className={`card ${className}`}>
                <h5 className="card-header">
                    <div className="row align-items-center">
                        {buttons.map((button, index) => (
                            <div className="col d-flex justify-content-center" key={index}>
                                <button
                                    className={`btn ${button.className}`}
                                    onClick={button.onClick}
                                    disabled={button.disabled}
                                >
                                    {button.text}
                                </button>
                            </div>
                        ))}
                    </div>

                </h5>
            </div>
        </>
    );
}

export default ButtonsComponent;