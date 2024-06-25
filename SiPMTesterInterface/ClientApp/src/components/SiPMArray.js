import {useContext, useState} from 'react';
import SiPMSensor from './SiPMSensor';
import { StatusEnum, getStatusBackgroundClass } from '../enums/StatusEnum';
import ArrayLocation from './ArrayLocation';
import ModeSelectButtonGroup from './ModeSelectButtonGroup';
import { MeasurementContext } from '../context/MeasurementContext';
import SiPMSettingsModal from './SiPMSettingsModal';
import { Button } from 'react-bootstrap';

function SiPMArray(props) {
    const { BlockIndex, ModuleIndex, ArrayIndex, SiPMCount, Editable, className } = props;
    const { measurementData, updateBarcode, updateSiPM, isAnyMeasurementRunning, measurementDataView } = useContext(MeasurementContext);
    const [showModal, setShowModal] = useState(false);

    const openModal = () => {
        setShowModal(true);
    };

    const closeModal = () => {
        setShowModal(false);
    };

    // Function to handle click event
    const handleClick = (property) => {
        updateSiPM(BlockIndex, ModuleIndex, ArrayIndex, undefined, property);
    };

    const handleBarcodeChange = (e) => {
        //console.log(e.target.value);
        updateBarcode(BlockIndex, ModuleIndex, ArrayIndex, e.target.value);
    };

    return (
        <>
            <SiPMSettingsModal showModal={showModal}  closeModal={closeModal} BlockIndex={BlockIndex} ModuleIndex={ModuleIndex} ArrayIndex={ArrayIndex} />
            <div className={`card ${className}`}>
                <h5 className="card-header">
                    <div className="row align-items-center justify-content-center">
                        <div className="col-auto">
                            <ArrayLocation arrayLocation={ArrayIndex} />
                        </div>

                        <div className="w-100 d-md-none mb-2"></div>

                        {/* Text Input */}
                        <div className="col">
                            <input
                                type="text"
                                id={`barcodeInput${ArrayIndex}`}
                                className={`form-control`}
                                placeholder={`Enter Barcode for Array ${ArrayIndex + 1}`}
                                value={measurementData.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].Barcode}
                                onChange={(e) => handleBarcodeChange(e)}
                                required // HTML5 form validation
                                pattern="\S+" // Ensures non-whitespace characters are entered
                                disabled={measurementDataView} // Disable input if no SiPM is selected in the array
                            />
                        </div>

                        <div className="w-100 d-md-none mb-2"></div>

                        {/* Button */}
                    
                        <div className="col-auto float-end">
                            <div className="btn-group" role="group" aria-label="sensor block with settings">
                                <ModeSelectButtonGroup BlockIndex={BlockIndex} ModuleIndex={ModuleIndex} ArrayIndex={ArrayIndex}> </ModeSelectButtonGroup>
                            </div>
                        </div>

                        <div className="col-auto">
                            <Button disabled={measurementDataView} onClick={openModal} variant="secondary"><i className="bi bi-gear"></i></Button>
                        </div>

                        <div className="col-auto float-end">
                            
                        </div>
                    </div>
                </h5>
                <div className="card-body">
                    <div className="container text-center">
                        <div className="row justify-content-center row-cols-auto">
                        {Array.from({ length: SiPMCount }, (_, j) => (
                            <div key={j} className="col p-1">
                                <SiPMSensor className="" BlockIndex={BlockIndex} ModuleIndex={ModuleIndex} ArrayIndex={ArrayIndex} SiPMIndex={j}>
                                </SiPMSensor>
                            </div>
                        ))}
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}

export default SiPMArray;
