import {useContext, useState} from 'react';
import SiPMSensor from './SiPMSensor';
import { StatusEnum, getStatusBackgroundClass } from '../enums/StatusEnum';
import ArrayLocation from './ArrayLocation';
import ModeSelectButtonGroup from './ModeSelectButtonGroup';
import { MeasurementContext } from '../context/MeasurementContext';
import SiPMSettingsModal from './SiPMSettingsModal';
import { Button, Dropdown } from 'react-bootstrap';

function SiPMArray(props) {
    const { BlockIndex, ModuleIndex, ArrayIndex, SiPMCount, Editable, className } = props;
    const { measurementStates, measurementData, updateBarcode, updateSiPM, isAnyMeasurementRunning, measurementDataView } = useContext(MeasurementContext);
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

    const downloadJSON = (data, filename) => {
        const json = JSON.stringify(data, null, 2);
        const blob = new Blob([json], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    };

    const downloadCSV = (data, filename) => {
        const csvRows = data.map(item => `${item.BreakdownVoltage},${item.CompensatedBreakdownVoltage}`).join('\n');
        const blob = new Blob([csvRows], { type: 'text/plain' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    };

    const handleExport = (format) => {
        const sipms = measurementStates.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs;
        const data = sipms.map((sipm) => ({
            BreakdownVoltage: sipm.IVAnalysationResult.BreakdownVoltage,
            CompensatedBreakdownVoltage: sipm.IVAnalysationResult.CompensatedBreakdownVoltage
        }));

        if (format === 'json') {
            downloadJSON(data, 'breakdown_voltages.json');
        } else if (format === 'text') {
            downloadCSV(data, 'breakdown_voltages.txt');
        }
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
                            {measurementDataView ? (
                                <Dropdown>
                                    <Dropdown.Toggle variant="secondary" id="dropdown-basic">
                                        <i className="bi bi-download"></i>
                                    </Dropdown.Toggle>

                                    <Dropdown.Menu>
                                        <Dropdown.Item onClick={() => handleExport('json')}>JSON</Dropdown.Item>
                                        <Dropdown.Item onClick={() => handleExport('text')}>Text</Dropdown.Item>
                                    </Dropdown.Menu>
                                </Dropdown>
                            ) : (
                                <Button disabled={measurementDataView} onClick={openModal} variant="secondary">
                                    <i className="bi bi-gear"></i>
                                </Button>
                            )}
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
