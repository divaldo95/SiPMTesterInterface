import { useContext } from 'react';
import { MeasurementContext } from '../context/MeasurementContext';
import { MessageTypeEnum, getIconClass } from '../enums/MessageTypeEnum';

function SiPMArray(props) {
    const { className } = props;
    const { messages, setDismissed } = useContext(MeasurementContext);

    const dismissMessage = (index) => {
        setDismissed(index, true);
    };

    return (
        <>
            <div aria-live="polite" aria-atomic="true" className="">
                <div className="toast-container position-fixed bottom-0 end-0 p-3">
                    {messages.map((message, index) => (
                        <div key={index}>
                            <div className={`mb-3 toast ${message.Dismissed ? '' : 'show'}`} role="alert" aria-live="assertive" aria-atomic="true">
                                <div className="toast-header">
                                    <div className={`me-2 ${getIconClass(message.MessageType)}`} alt="..."></div>
                                    <strong className="me-auto">{message.MessageType}</strong>
                                    <small className="text-body-secondary">just now</small>
                                    <button onClick={() => dismissMessage(index)} type="button" className="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                                </div>
                                <div className="toast-body">
                                    {message.Message}
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </>
    );
}

export default SiPMArray;
