﻿import React, { useContext, useState } from 'react';
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

    const { measurementData, measurementStates, updateSiPM, isAnyMeasurementRunning, addToast, measurementDataView } = useContext(MeasurementContext);
    //console.log(measurementData);

    // Function to handle click event
    const handleClick = (property) => {
        updateSiPM(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, property);
    };

    const toggleSiPM = () => {
        if (measurementDataView) {
            addToast(MessageTypeEnum.Debug, "SiPM pressed in measurement view");
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
        //console.log(measurementStates.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs[SiPMIndex]);
        try {
            return measurementStates.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs[SiPMIndex][property];
        }
        catch {
            console.log("Cannot find property: " + property);
            return null;
        }
        
    }

    const getAnalysationBackgroundClass = () => {
        let result = getSiPMMeasurementValue("IVAnalysationResult");
        //console.log(result);
        if (result === null) {
            //console.log("null bg");
            return "bg-secondary";
        }
        //console.log(result);
        if (!result.Analysed) {
            //console.log("not analysed bg");
            return "bg-light";
        }
        if (result.IsOK) {
            //console.log("ok bg");
            return "bg-success";
        }
        else {
            //console.log("error bg");
            return "bg-danger";
        }
    }

    const getChecksBackgroundClass = () => {
        let result = getSiPMMeasurementValue("Checks");
        if (result === null) {
            //console.log("null bg");
            return "bg-secondary";
        }

        const allDone =
            result.RForwardDone &&
            result.IDarkDone &&
            result.IVDone;

        const allChecksPassed =
            result.DMMResistanceOK && result.RForwardOK &&
            result.IDarkOK && result.IVTemperatureOK &&
            result.IVMeasurementOK && result.IVVoltageCheckOK &&
            result.IVCurrentCheckOK && result.IVVbrOK;

        if (allDone) {
            if (allChecksPassed) {
                return "bg-success";
            }
            else {
                return "bg-danger";
            }
        }
        else {
            if (result.IVDone) {
                return "bg-secondary"; //most of the time not reached because allDone should be true when IV measurement finishes
            }
            else if (result.RForwardDone) {
                return "bg-info";
            }
            else if (result.IDarkDone) {
                return "bg-primary";
            }
            else {
                return "bg-light";
            }
            
        }
    }

    const getSelectedBorderClass = () => {
        if (measurementDataView) {
            let result = getSiPMMeasurementValue("Checks");
            if (result === null) {
                console.log("null border");
                return "";
            }

            if (result.SelectedForMeasurement) {
                return "border-info";
            }
        }
        else {
            return "";
        }
    }

    const backgroundClass = () => {
        if (measurementDataView) {
            return getChecksBackgroundClass();
        }
        else {
            return GetSelectedColorClass(getSiPMValue("IV"), getSiPMValue("SPS"));
        }
    }

    const borderClass = () => {
        if (measurementDataView) {
            return GetStatusBorderColorClass(getSiPMMeasurementValue("IVMeasurementDone"), getSiPMMeasurementValue("SPSMeasurementDone"));
        }
        else {
            return "";
        }
    }

    const GetBreakdownVoltage = () => {
        let Vbr = 0.0;
        let result = getSiPMMeasurementValue("IVAnalysationResult");
        if (result === null) {
            return Vbr;
        }
        else {
            return result.BreakdownVoltage;
        }
    }

    const GetCompensatedBreakdownVoltage = () => {
        let cVbr = 0.0;
        let result = getSiPMMeasurementValue("IVAnalysationResult");
        if (result === null) {
            return cVbr;
        }
        else {
            return result.CompensatedBreakdownVoltage;
        }
    }

    const GetChiSquare = () => {
        let c2 = 0.0;
        let result = getSiPMMeasurementValue("IVAnalysationResult");
        if (result === null) {
            return c2;
        }
        else {
            return result.ChiSquare;
        }
    }

    const GetChecks = () => {
        return getSiPMMeasurementValue("Checks");
    }

    return (
        <>
            <div className={`btn-group border border-1 ${getSelectedBorderClass()} ${className}`} role="group" aria-label="sensor block with settings">
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
                    <i className={`bi ${measurementDataView ? 'bi-caret-down' : 'bi-gear'}`}></i>
                </button>
            </div>
            {measurementDataView ? (
                <SiPMMeasurementModal
                    showModal={showModal}
                    closeModal={closeModal}
                    BlockIndex={BlockIndex}
                    ModuleIndex={ModuleIndex}
                    ArrayIndex={ArrayIndex}
                    SiPMIndex={SiPMIndex}
                    BreakdownVoltage={GetBreakdownVoltage()}
                    CompensatedBreakdownVoltage={GetCompensatedBreakdownVoltage()}
                    Chi2={GetChiSquare()}
                    Checks={GetChecks()}
                />
            ) : (
                <SiPMSettingsModal showModal={showModal} closeModal={closeModal} BlockIndex={BlockIndex} ModuleIndex={ModuleIndex} ArrayIndex={ArrayIndex} SiPMIndex={SiPMIndex} />
            )}
        </>
    );
}

export default SiPMSensor;