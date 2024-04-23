import { useRef } from 'react';
import OverlayTrigger from 'react-bootstrap/OverlayTrigger';
import Tooltip from 'react-bootstrap/Tooltip';

function OneButtonCard(props) {
    const { title, buttonText, buttonColor, textColor, clickFunction, className, tooltipMessage, buttonIcon } = props

    const renderTooltip = (props) => (
        <Tooltip id="button-tooltip" {...props}>
            {tooltipMessage}
        </Tooltip>
    );

    return (
        <>
            <div className={`card ${className}`}>
                <h5 className="card-header">{title}</h5>
                <div className="card-body d-flex align-items-center justify-content-center">
                    <div className="container text-center">
                        <div className="btn-group" role="group" aria-label="sensor block with settings">
                            <OverlayTrigger
                                placement="bottom"
                                delay={{ show: 250, hide: 400 }}
                                overlay={renderTooltip}
                            >
                                <button disabled={false} onClick={(e) => { e.preventDefault(); clickFunction(); }} className={`btn ${buttonColor} ${textColor}`}>
                                    {buttonText} {buttonIcon !== undefined ? <i className={`ms-1 bi ${buttonIcon}`}></i> : ''}
                                </button>
                            </OverlayTrigger>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}

export default OneButtonCard;