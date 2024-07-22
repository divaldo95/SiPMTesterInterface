import { useContext, useState, useCallback } from 'react';
import SiPMSensor from './SiPMSensor';
import { StatusEnum, getStatusBackgroundClass } from '../enums/StatusEnum';
import ArrayLocation from './ArrayLocation';
import ModeSelectButtonGroup from './ModeSelectButtonGroup';
import { MeasurementContext } from '../context/MeasurementContext';
import ArraySettingsModal from './ArraySettingsModal';
import { Button, Dropdown, InputGroup, FormControl, Spinner } from 'react-bootstrap';
import debounce from 'lodash.debounce';
import MeasurementStateService from '../services/MeasurementStateService';

function SiPMArray(props) {
    const { BlockIndex, ModuleIndex, ArrayIndex, SiPMCount, Editable, className } = props;
    const { measurementStates, measurementData, updateBarcode, updateSiPM, isAnyMeasurementRunning, measurementDataView, updateVopData } = useContext(MeasurementContext);
    const [showModal, setShowModal] = useState(false);
    const [isBarcodeFetching, setIsBarcodeFetching] = useState(false);

    

    const openModal = () => {
        setShowModal(true);
    };

    const closeModal = () => {
        setShowModal(false);
    };

    const UpdateVopsAndEnable = (serverResponse) => {
        serverResponse.Channels.map((channel, index) => {
            updateVopData(BlockIndex, ModuleIndex, ArrayIndex, channel.ChNo - 1, channel.Vop);
        });
    }

    const fillVopWithZero = () => {
        for (let i = 0; i < measurementData.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs.length; i++) {
            updateVopData(BlockIndex, ModuleIndex, ArrayIndex, i, 0.0);
        }
    };

    const handleSearch = (barcode) => {
        MeasurementStateService.getArrayPropertiesBySN(barcode)
            .then((resp) => {
                console.log(resp);
                UpdateVopsAndEnable(resp);
                setIsBarcodeFetching(false);
            })
            .catch((err) => {
                setIsBarcodeFetching(false);
                fillVopWithZero();
                console.error(err);
            });
    };

    const debounceFn = useCallback(debounce(handleSearch, 2000), []);

    const isAllSelectedHasVop = measurementData.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs.some(sipm => sipm.IV > 0 && sipm.OperatingVoltage < 20.0);

    // Function to handle click event
    const handleClick = (property) => {
        updateSiPM(BlockIndex, ModuleIndex, ArrayIndex, undefined, property);
    };

    const handleBarcodeChange = (e) => {
        //console.log(e.target.value);
        updateBarcode(BlockIndex, ModuleIndex, ArrayIndex, e.target.value);
        debounceFn(e.target.value);
        setIsBarcodeFetching(true);
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
            <ArraySettingsModal showModal={showModal} closeModal={closeModal} BlockIndex={BlockIndex} ModuleIndex={ModuleIndex} ArrayIndex={ArrayIndex} handleBarcodeChange={handleBarcodeChange} isBarcodeFetching={isBarcodeFetching} />
            <div className={`card ${className}`}>
                <h5 className="card-header">
                    <div className="row align-items-center justify-content-center">
                        <div className="col-auto">
                            <ArrayLocation arrayLocation={ArrayIndex} />
                        </div>

                        <div className="w-100 d-md-none mb-2"></div>

                        {/* Text Input */}
                        <div className="col">
                            <InputGroup>
                                <FormControl
                                    type="text"
                                    id={`barcodeInput${ArrayIndex}`}
                                    className="form-control"
                                    placeholder={`Enter Barcode for Array ${ArrayIndex + 1}`}
                                    value={measurementData.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].Barcode}
                                    onChange={(e) => handleBarcodeChange(e)}
                                    required // HTML5 form validation
                                    pattern="\S+" // Ensures non-whitespace characters are entered
                                    disabled={measurementDataView} // Disable input if no SiPM is selected in the array
                                />
                                {isBarcodeFetching && (
                                    <InputGroup.Text>
                                        <Spinner animation="border" size="sm" />
                                    </InputGroup.Text>
                                )}
                            </InputGroup>
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
                                    <Button disabled={measurementDataView} onClick={openModal} variant={isAllSelectedHasVop ? "secondary" : "success"}>
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
