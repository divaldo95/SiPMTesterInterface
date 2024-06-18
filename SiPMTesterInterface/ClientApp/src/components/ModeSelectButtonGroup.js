import React, { useContext } from 'react';
import { GetSiPMNumber, GetSiPMLocation } from "./HelperMethods";
import { StatusEnum, getStatusBackgroundClass, GetSelectedColorClass } from '../enums/StatusEnum';
import './SiPMArray.css';
import { MeasurementContext } from '../context/MeasurementContext';

function ModeSelectButtonGroup(props) {
    const { BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex } = props;

    const { measurementData, updateSiPM, areAllPropertiesSet, isAnyMeasurementRunning, measurementDataView } = useContext(MeasurementContext);
    //console.log(measurementData);

    // Function to handle click event
    const handleClick = (property) => {
        updateSiPM(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, property);
    };

    const isAllSets = () => {
        return areAllPropertiesSet(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex);
    }

    return (
        <>
            <div className="btn-group" role="group" aria-label="sensor block with settings">
                <button disabled={measurementDataView} onClick={(e) => { e.preventDefault(); handleClick("IV"); }} className={`btn ${isAllSets().allIVSet ? 'btn-success' : 'btn-secondary'}`}>
                    IV <i className={`ms-1 bi ${isAllSets().allIVSet ? 'bi-dash-square' : 'bi-check-square'}`}></i>
                </button>
                <button disabled={measurementDataView} onClick={(e) => { e.preventDefault(); handleClick("SPS"); }} className={`btn ${isAllSets().allSPSSet ? 'btn-success' : 'btn-secondary'}`}>
                    SPS <i className={`ms-1 bi ${isAllSets().allSPSSet ? 'bi-dash-square' : 'bi-check-square'}`}></i>
                </button>
                <button disabled={measurementDataView} onClick={(e) => { e.preventDefault(); handleClick("Both"); }} className={`btn ${isAllSets().allIVSet && isAllSets().allSPSSet ? 'btn-success' : 'btn-secondary'}`}>
                    Both <i className={`ms-1 bi ${isAllSets().allIVSet && isAllSets().allSPSSet ? 'bi-dash-square' : 'bi-check-square'}`}></i>
                </button>
            </div>
        </>
    );
}

export default ModeSelectButtonGroup;
