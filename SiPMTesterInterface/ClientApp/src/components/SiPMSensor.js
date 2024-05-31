import React, { useContext, useState } from 'react';
import { GetSiPMNumber, GetSiPMLocation } from "./HelperMethods";
import { StatusEnum, getStatusBackgroundClass, GetSelectedColorClass, GetMeasurementStateColorClass, GetStatusBorderColorClass } from '../enums/StatusEnum';
import { MessageTypeEnum, getIconClass } from '../enums/MessageTypeEnum';
import './SiPMArray.css';
import SiPMSettingsModal from './SiPMSettingsModal';
import { MeasurementContext } from '../context/MeasurementContext';
import SiPMMeasurementModal from './SiPMMeasurementModal';
import { Button, Badge } from 'react-bootstrap';

function SiPMSensor(props) {
    const { BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, className } = props;

    const [showModal, setShowModal] = useState(false);

    const openModal = () => {
        setShowModal(true);
    };

    const closeModal = () => {
        setShowModal(false);
    };

    const { measurementData, measurementStates, updateSiPM, isAnyMeasurementRunning, addToast } = useContext(MeasurementContext);
    //console.log(measurementData);

    // Function to handle click event
    const handleClick = (property) => {
        updateSiPM(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, property);
    };

    const toggleSiPM = () => {
        if (isAnyMeasurementRunning()) {
            addToast(MessageTypeEnum.Debug, "SiPM pressed while a measurement is running");
            return;
        }

        const iv = getSiPMValue("IV");
        const sps = getSiPMValue("SPS");
        //console.log("iv:" + iv + " sps: " + sps);
        if (iv === 0 && sps === 0) {
            updateSiPM(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, "IV");
        }
        else if (iv === 1 && sps === 0) {
            updateSiPM(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, "IV");
            updateSiPM(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, "SPS");
        }
        else if (iv === 0 && sps === 1) {
            updateSiPM(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, "IV");
        }
        else if (iv === 1 && sps === 1) {
            updateSiPM(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, "IV");
            updateSiPM(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, "SPS");
        }
    }

    // Function to get SiPM value
    const getSiPMValue = (property) => {
        return measurementData.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs[SiPMIndex][property];
    };

    const getSiPMMeasurementValue = (property) => {
        return measurementStates.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs[SiPMIndex][property];
    }

    const isMeasurementRunning = () => {
        return isAnyMeasurementRunning();
    }

    const backgroundClass = () => {
        if (isAnyMeasurementRunning()) {
            return GetMeasurementStateColorClass();
        }
        else {
            return GetSelectedColorClass(getSiPMValue("IV"), getSiPMValue("SPS"));
        }
    }

    const borderClass = () => {
        if (isAnyMeasurementRunning()) {
            return GetStatusBorderColorClass(getSiPMMeasurementValue("IVMeasurementDone"), getSiPMMeasurementValue("SPSMeasurementDone"));
        }
        else {
            return "";
        }
    }

    return (
        <>
            <div className={`btn-group border border-1 ${borderClass()} ${className}`} role="group" aria-label="sensor block with settings">
                <button
                    type="button"
                    key={GetSiPMNumber(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex)}
                    className={`btn btn-block btn-sm ${backgroundClass()}`}
                    onClick={() => toggleSiPM()}
                >
                    {SiPMIndex}
                </button>
                <button
                    type="button"
                    className={`btn btn-block btn-sm ${backgroundClass()}`}
                    onClick={() => openModal()}
                >
                    <i className={`bi ${isAnyMeasurementRunning() ? 'bi-caret-down' : 'bi-gear'}`}></i>
                </button>
            </div>
            {isAnyMeasurementRunning() ? (
                <SiPMMeasurementModal showModal={showModal} closeModal={closeModal} BlockIndex={BlockIndex} ModuleIndex={ModuleIndex} ArrayIndex={ArrayIndex} SiPMIndex={SiPMIndex} />
            ) : (
                <SiPMSettingsModal showModal={showModal} closeModal={closeModal} BlockIndex={BlockIndex} ModuleIndex={ModuleIndex} ArrayIndex={ArrayIndex} SiPMIndex={SiPMIndex} />
            )}
        </>
    );
}

export default SiPMSensor;