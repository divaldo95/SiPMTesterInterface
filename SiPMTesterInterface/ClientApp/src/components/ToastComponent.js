import { useContext } from 'react';
import Toast from 'react-bootstrap/Toast';
import ToastContainer from 'react-bootstrap/ToastContainer';
import { useMeasurement } from '../context/MeasurementContext';
import { MessageTypeEnum, getIconClass } from '../enums/MessageTypeEnum';

function ToastComponent(props) {
    const { className } = props;
    const { toasts, dismissToast } = useMeasurement();

    return (
        <div
            aria-live="polite"
            aria-atomic="true"
            className=""
        >
            <ToastContainer position="bottom-end" className="position-fixed m-3" style={{ zIndex: 1 }}>
                {toasts
                    .filter((toast) => !toast.dismissed)
                    .map((toast) => (
                    <Toast
                        key={toast.id}
                        onClose={() => dismissToast(toast.id)}
                        delay={5000}
                        autohide
                        animation={true}
                    >
                        <Toast.Header>
                            <div className={`me-2 ${getIconClass(toast.messageType)}`} alt="..."></div>
                            <strong className="me-auto">{toast.messageType}</strong>
                                <small className="text-body-secondary">{new Date(toast.dateTime).toLocaleDateString()} {new Date(toast.dateTime).toTimeString().split(' ')[0]}</small>
                        </Toast.Header>
                        <Toast.Body>
                            <div className="text-wrap">
                                {toast.message}
                            </div>
                        </Toast.Body>
                    </Toast>
                ))}
            </ToastContainer>
        </div>
    );
}

export default ToastComponent;
