import { useContext, useState } from 'react';
import { MeasurementContext } from '../context/MeasurementContext';

function VoltageListComponent(props) {
    const { BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, MeasurementMode, className, handleNewList } = props;
    const [startValue, setStartValue] = useState(25.5);
    const [endValue, setEndValue] = useState(30.5);
    const [stepValue, setStepValue] = useState(0.1);
    const [voltageList, setVoltageList] = useState([]);

    const { measurementData, updateVoltages, addToast } = useContext(MeasurementContext);

    const getCurrentListValue = () => {
        if (BlockIndex !== undefined && ModuleIndex !== undefined && ArrayIndex !== undefined && SiPMIndex !== undefined) {
            if (MeasurementMode === 'IV') {
                return measurementData.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs[SiPMIndex].IVVoltages;
            }
            else if (MeasurementMode === 'SPS') {
                return measurementData.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs[SiPMIndex].SPSVoltages;
            }
        }
        else {
            if (MeasurementMode === 'IV') {
                return measurementData.IVVoltages;
            }
            else if (MeasurementMode === 'SPS') {
                return measurementData.SPSVoltages;
            }
        }
        return [];
    };

    const actualVoltageList = getCurrentListValue();

    const sortAndSetVoltageList = (list) => {
        const sortedList = list.map(parseFloat).sort((a, b) => a - b);
        console.log(sortedList);
        setVoltageList(sortedList);
        //handleNewList(sortedList);
        updateVoltages(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, MeasurementMode, sortedList, false);
        console.log(measurementData);
    };

    const handleGenerationDataChange = () => {

        //updateGeneratorData(startValue, endValue, stepValue);
    };

    const handleGenerate = (e) => {
        e.preventDefault();

        const start = parseFloat(startValue);
        const end = parseFloat(endValue);
        const step = parseFloat(stepValue);

        if (!isNaN(start) && !isNaN(end) && !isNaN(step) && step > 0) {
            const generatedVoltages = [];
            for (let i = start; i <= end; i += step) {
                generatedVoltages.push(i.toFixed(2));
            }
            sortAndSetVoltageList(generatedVoltages);
        }
    };

    const handleAddLine = (e) => {
        e.preventDefault();
        sortAndSetVoltageList([...voltageList, 0.01]);
    };

    const emptyVoltageList = (e) => {
        e.preventDefault();
        sortAndSetVoltageList([]);
    };

    const handleRemoveLine = (e, index) => {
        e.preventDefault();
        const updatedList = [...voltageList];
        updatedList.splice(index, 1);
        sortAndSetVoltageList(updatedList);
    };

    const handleLineChange = (index, value) => {
        // No need to preventDefault for onBlur
        const updatedList = [...voltageList];
        updatedList[index] = value;
        // Not sorting here to avoid reordering on every input change
        // Sorting will be done onBlur
        setVoltageList(updatedList);
    };

    const handleLineBlur = () => {
        // Sort the list when an input loses focus
        sortAndSetVoltageList(voltageList);
    };

    return (
        <div className={`${className}`}>
            <div className="row justify-content-center mb-4">
                <div className="col">
                    <div className="card">
                        <h5 className="card-header">{MeasurementMode} Voltage List Generator</h5>
                        <div className="card-body">
                            <div className="row">
                                <div className="col">
                                    <label htmlFor="startInput" className="form-label">
                                        Start Voltage
                                    </label>
                                    <input
                                        type="number"
                                        min="0"
                                        step="0.01"
                                        className="form-control"
                                        id="startInput"
                                        value={startValue}
                                        onChange={(e) => {
                                            setStartValue(e.target.value);
                                            handleGenerationDataChange(); // Call your function here
                                        }}
                                    />
                                </div>

                                <div className="col">
                                    <label htmlFor="endInput" className="form-label">
                                        End Voltage
                                    </label>
                                    <input
                                        type="number"
                                        min="0"
                                        step="0.01"
                                        className="form-control"
                                        id="endInput"
                                        value={endValue}
                                        onChange={(e) => {
                                            setEndValue(e.target.value);
                                            handleGenerationDataChange(); // Call your function here
                                        }}
                                    />
                                </div>

                                <div className="col">
                                    <label htmlFor="stepInput" className="form-label">
                                        Step Value
                                    </label>
                                    <input
                                        type="number"
                                        min="0.01"
                                        step="0.01"
                                        className="form-control"
                                        id="stepInput"
                                        value={stepValue}
                                        onChange={(e) => {
                                            setStepValue(e.target.value);
                                            handleGenerationDataChange(); // Call your function here
                                        }}
                                    />
                                </div>
                            </div>
                            <div className="container">
                                <div className="row text-center">
                                    <div className="col p-3">
                                        <button className="btn btn-primary"
                                            onClick={handleGenerate}
                                        >
                                            Generate
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div className="row justify-content-center mb-4">
                <div className="col">
                    <div className="card">
                        <h5 className="card-header d-flex justify-content-between align-items-center">
                            {MeasurementMode} Voltages
                            <div>
                                <button className="btn btn-danger btn-sm mx-1" onClick={emptyVoltageList}>
                                    <i className="bi bi-trash"></i>
                                </button>
                                <button className="btn btn-success btn-sm" onClick={handleAddLine}>
                                    <i className="bi bi-plus"></i>
                                </button>
                            </div>
                        </h5>
                        <div className="card-body">
                            <ul className="list-group">
                                {actualVoltageList.map((voltage, index) => (
                                    <li key={index} className="input-group-sm mb-1 d-flex justify-content-between align-items-center">
                                        <input
                                            type="number"
                                            min="0.01"
                                            step="0.01"
                                            className="form-control"
                                            value={voltage}
                                            onChange={(e) => handleLineChange(index, e.target.value)}
                                            onBlur={handleLineBlur}
                                        />
                                        <button className="btn btn-danger" onClick={(e) => handleRemoveLine(e, index)}>
                                            <i className="bi bi-trash"></i>
                                        </button>
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default VoltageListComponent;
