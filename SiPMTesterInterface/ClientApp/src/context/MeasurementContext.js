import React, { useContext, createContext, useState } from 'react';
import MeasurementStateService from '../services/MeasurementStateService';
import { TaskTypes } from '../enums/TaskTypes';


export const MeasurementContext = createContext();
export const useMeasurement = () => useContext(MeasurementContext);

const initialState = {
    Blocks: Array.from({ length: 4 }, () => ({
        Modules: Array.from({ length: 2 }, () => ({
            Arrays: Array.from({ length: 4 }, () => ({
                SiPMs: Array.from({ length: 16 }, () => ({
                    DMMResistance: 0,
                    IV: 0,
                    DarkCurrent: 0,
                    ForwardResistance: 0,
                    IVVoltages: [],
                    SPS: 0,
                    SPSVoltages: [],
                    SPSVoltagesIsOffsets: 0,
                    OperatingVoltage: 0.0,
                })),
                Barcode: ""
            }))
        }))
    })),
    IV: 0,
    SPS: 0,
    DarkCurrent: 0,
    ForwardResistance: 0,
    DMMResistance: {
        CorrectionPercentage: 10,
        Iterations: 3,
        Voltage: 30.0
    },
    IVVoltages: [],
    SPSVoltages: [],
    MeasureDMMResistance: true
};

const initialMeasurementState = {
    ActiveSiPMs: [],
    Blocks: Array.from({ length: 5 }, () => ({
        Modules: Array.from({ length: 2 }, () => ({
            Arrays: Array.from({ length: 4 }, () => ({
                SiPMs: Array.from({ length: 16 }, () => ({
                    IVMeasurementDone: false,
                    SPSMeasurementDone: false,
                    IVAnalysationResult: {
                        Analysed: false,
                        IsCurrentCheckOK: false,
                        IsOK: false,
                        BreakdownVoltage: 0.0,
                        ChiSquare: 0.0,
                        CompensatedBreakdownVoltage: 0.0,
                        RootFileLocation: ""
                    },
                    Checks: {
                        SelectedForMeasurement: false,
                        DMMResistanceOK: false,

                        RForwardDone: false,
                        RForwardOK: false,

                        IDarkDone: false,
                        IDarkOK: false,

                        IVDone: false,
                        IVTemperatureOK: false,
                        IVMeasurementOK: false,
                        IVVoltageCheckOK: false,
                        IVCurrentCheckOK: false,
                        IVVbrOK: false
                    },
                    SPSResult: {
                        IsOK: false,
                        Gain: 0.0
                    },
                    IVTimes: {
                        StartTimestamp: 0,
                        EndTimestamp: 0
                    }
                })),
            }))
        }))
    }))
};

const generateCoolerData = (blockNum, moduleNum) => {
    const coolerSettings = [];
    for (let block = 0; block < blockNum; block++) {
        for (let module = 0; module < moduleNum; module++) {
            coolerSettings.push({
                Block: block,
                Module: module,
                Enabled: false,
                TargetTemperature: 0.0,
                FanSpeed: 0,
                State: {
                    Block: block,
                    Module: module,
                    State: 0,
                    IsTemperatureStable: false,
                    CoolerTemperature: 0.0,
                    PeltierVoltage: 0.0,
                    PeltierCurrent: 0.0,
                    Timestamp: Date.now(),
                },
                Temperature: Array(8).fill(0.0)
            });
        }
    }
    return coolerSettings;
};

const initialInstrumentStates = {
    CurrentTask: TaskTypes.Idle,
    IVConnectionState: 0,
    SPSConnectionState: 0,
    IVState: 0,
    SPSState: 0
};

const initialToasts = [
];

export const MeasurementProvider = ({ children }) => {
    const [measurementData, setMeasurementData] = useState(initialState);
    const [tempMeasurementData, setTempMeasurementData] = useState(initialState);
    const [measurementStates, setMeasurementStates] = useState(initialMeasurementState);
    const [isIVMeasurementRunning, setIsIVMeasurementRunning] = useState(true);
    const [isSPSMeasurementRunning, setIsSPSMeasurementRunning] = useState(false);
    const [measurementDataView, setMeasurementDataView] = useState(false);

    const [showLogModal, setShowLogModal] = useState(false);
    const [showPulserLEDModal, setShowPulserLEDModal] = useState(false);
    const [showExportExcelModal, setShowExportExcelModal] = useState(false);

    const [pulserState, setPulserState] = useState(true);
    const [toasts, setToasts] = useState(initialToasts);

    const [instrumentStatuses, setInstrumentStatuses] = useState(initialInstrumentStates);

    const initialCoolerData = {
        BlockNum: 2,
        ModuleNum: 2,
        CoolerSettings: generateCoolerData(2, 2),
    };

    const [coolerStateHandler, setCoolerStateHandler] = useState(initialCoolerData);

    const [showMeasurementWizard, setShowMeasurementWizard] = useState(false);

    const deepClone = (obj) => JSON.parse(JSON.stringify(obj));

    const handleShowMeasurementWizard = () => {
        const blankedOutData = JSON.parse(JSON.stringify(measurementData));
        blankedOutData.Blocks.forEach(block => {
            block.Modules.forEach(module => {
                module.Arrays.forEach(array => {
                    array.Barcode = "";
                });
            });
        });

        setMeasurementData(blankedOutData);
        setShowMeasurementWizard(true);
    };

    const handleCloseMeasurementWizard = () => {
        setShowMeasurementWizard(false);
    };

    const handleSaveBarcodeChanges = () => {
        setShowMeasurementWizard(false);
    };

    const updateCoolerStateHandler = (newState) => {
        setCoolerStateHandler(newState);
    };

    const updateCoolerData = (newData) => {
        setCoolerStateHandler((prevState) => ({
            ...prevState,
            coolerSettings: newData,
        }));
    };

    const handleShowLogModal = () => setShowLogModal(true);
    const handleCloseLogModal = () => setShowLogModal(false);

    const handleShowPulserLEDModal = () => setShowPulserLEDModal(true);
    const handleClosePulserLEDModal = () => setShowPulserLEDModal(false);

    const handleShowExcelExportModal = () => setShowExportExcelModal(true);
    const handleCloseExcelExportModal = () => setShowExportExcelModal(false);

    const addToast = (messageType, message) => {
        const newToast = {
            id: Date.now(),
            messageType,
            message,
            dismissed: false,
            dateTime: Date.now()
        };
        setToasts((prevToasts) => [...prevToasts, newToast]);
    };

    const dismissToast = (id) => {
        setToasts((prevToasts) =>
            prevToasts.map((toast) =>
                toast.id === id ? { ...toast, dismissed: true } : toast
            )
        );
    };

    const checkBarcodes = () => {
        for (const block of measurementData.Blocks) {
            for (const module of block.Modules) {
                for (const array of module.Arrays) {
                    // Check if any SiPM in the array has IV or SPS greater than 0
                    const hasRelevantSiPM = array.SiPMs.some(sipm => sipm.IV > 0 || sipm.SPS > 0);
                    if (hasRelevantSiPM && (!array.Barcode || !array.Barcode.trim())) {
                        return false;
                    }
                }
            }
        }
        return true;
    };

    const checkCheckedSiPMs = () => {
        for (const block of measurementData.Blocks) {
            for (const module of block.Modules) {
                for (const array of module.Arrays) {
                    // Check if any SiPM in the array has IV or SPS greater than 0
                    const hasRelevantSiPM = array.SiPMs.some(sipm => sipm.IV > 0 || sipm.SPS > 0);
                    if (hasRelevantSiPM) {
                        return true;
                    }
                }
            }
        }
        return false;
    };

    const canMeasurementStart = () => {
        return (checkCheckedSiPMs() && checkBarcodes());
    };

    const fetchCurrentRun = () => {
        try {
            const data = MeasurementStateService.getCurrentRun()
                .then((resp) => {
                    if (resp.Blocks.length > 0) {
                        setMeasurementData(resp);
                    }
                    
                    console.log(resp);
                })
        } catch (error) {
            console.error('Error fetching current run data:', error);
        }
        
    }

    const updateSiPMMeasurementStates = (blockIndex, moduleIndex, arrayIndex, sipmIndex, property, newData) => {
        setMeasurementStates(prevMeasurementData => {
            // If no specific indices and property are provided, replace the entire structure
            if (
                blockIndex === undefined &&
                moduleIndex === undefined &&
                arrayIndex === undefined &&
                sipmIndex === undefined &&
                property === undefined
            ) {
                return newData; // Replace the entire state with the provided newData
            }

            // Otherwise, perform the partial update on the specific SiPM
            const updatedData = {
                ...prevMeasurementData, // Make a shallow copy of the previous state
                Blocks: prevMeasurementData.Blocks.map((block, bIdx) => {
                    if (bIdx !== blockIndex) return block; // If blockIndex does not match, return the block unchanged

                    return {
                        ...block, // Shallow copy the block
                        Modules: block.Modules.map((module, mIdx) => {
                            if (mIdx !== moduleIndex) return module; // If moduleIndex does not match, return the module unchanged

                            return {
                                ...module, // Shallow copy the module
                                Arrays: module.Arrays.map((array, aIdx) => {
                                    if (aIdx !== arrayIndex) return array; // If arrayIndex does not match, return the array unchanged

                                    return {
                                        ...array, // Shallow copy the array
                                        SiPMs: array.SiPMs.map((sipm, sIdx) => {
                                            if (sIdx !== sipmIndex) return sipm; // If sipmIndex does not match, return the SiPM unchanged

                                            return {
                                                ...sipm, // Shallow copy the SiPM
                                                [property]: newData // Update only the specific property with the new data
                                            };
                                        })
                                    };
                                })
                            };
                        })
                    };
                })
            };

            return updatedData; // Return the updated state
        });
    };


    const updateActiveSiPMs = (newData) => {
        setMeasurementStates((prevMeasurementData) => ({
            ...prevMeasurementData,
            ActiveSiPMs: newData,
        }));
    };


    const resetSiPMMeasurementStates = () => {
        setMeasurementStates(initialMeasurementState);
    };

    /*
    const updateIVMeasurementIsRunning = (state) => {
        if (state === 1 || state === true) {
            setIsIVMeasurementRunning(true);
        }
        else {
            setIsIVMeasurementRunning(false);
        }
    }
    */

    const updateMeasurementStates = (states) => {
        setMeasurementStates(states);
        //console.log(states);
    };

    const updateMeasurementDataView = (prevState, currState) => {
        if (prevState === TaskTypes.Idle && currState !== TaskTypes.Idle) {
            setMeasurementDataView(true);
        }
    };

    const updateInstrumentStates = (states) => {
        updateMeasurementDataView(instrumentStatuses.CurrentTask, states.CurrentTask);
        setInstrumentStatuses(states);
        //console.log(states);
    };

    const updateCurrentTask = (currentTask) => {
        updateMeasurementDataView(instrumentStatuses.CurrentTask, currentTask);
        setInstrumentStatuses(prevStatuses => {
            const updatedStatuses = prevStatuses;
            updatedStatuses["CurrentTask"] = currentTask;
            return updatedStatuses;
        });
        //console.log(states);
    };

    const updateSiPMMeasurementState = (blockIndex, moduleIndex, arrayIndex, sipmIndex, property, newState) => {
        setMeasurementStates(prevMeasurementState => {
            // Create a deep copy of the entire state
            const updatedState = { ...prevMeasurementState, [property]: newState };

            return updatedState;
        });
    };


    const updateVopData = (BlockIndex, ModuleIndex, ArrayIndex, index, Vop) => {
        console.log(BlockIndex, ModuleIndex, ArrayIndex, index, Vop);
        setMeasurementData(prevMeasurementState => {
            const updatedState = { ...prevMeasurementState };
            updatedState.Blocks = [...prevMeasurementState.Blocks];
            updatedState.Blocks[BlockIndex] = {
                ...prevMeasurementState.Blocks[BlockIndex],
                Modules: [...prevMeasurementState.Blocks[BlockIndex].Modules]
            };
            updatedState.Blocks[BlockIndex].Modules[ModuleIndex] = {
                ...prevMeasurementState.Blocks[BlockIndex].Modules[ModuleIndex],
                Arrays: [...prevMeasurementState.Blocks[BlockIndex].Modules[ModuleIndex].Arrays]
            };
            updatedState.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex] = {
                ...prevMeasurementState.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex],
                SiPMs: [...prevMeasurementState.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs]
            };
            updatedState.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs[index] = {
                ...prevMeasurementState.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs[index],
                OperatingVoltage: Vop,
                IV: (Vop > 20.0 ? 1 : 0)
            };
            return updatedState;
        });
    };

    const updateVopDataTemp = (BlockIndex, ModuleIndex, ArrayIndex, index, Vop) => {
        setTempMeasurementData(prevMeasurementState => {
            const updatedState = { ...prevMeasurementState };
            updatedState.Blocks = [...prevMeasurementState.Blocks];
            updatedState.Blocks[BlockIndex] = {
                ...prevMeasurementState.Blocks[BlockIndex],
                Modules: [...prevMeasurementState.Blocks[BlockIndex].Modules]
            };
            updatedState.Blocks[BlockIndex].Modules[ModuleIndex] = {
                ...prevMeasurementState.Blocks[BlockIndex].Modules[ModuleIndex],
                Arrays: [...prevMeasurementState.Blocks[BlockIndex].Modules[ModuleIndex].Arrays]
            };
            updatedState.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex] = {
                ...prevMeasurementState.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex],
                SiPMs: [...prevMeasurementState.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs]
            };
            updatedState.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs[index] = {
                ...prevMeasurementState.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs[index],
                OperatingVoltage: Vop,
                IV: (Vop > 20.0 ? 1 : 0)
            };
            return updatedState;
        });
    };

    const updateMeasurementData = (newData) => {
        setMeasurementData({ ...measurementData, ...newData });
    };

    const updateVoltages = (blockIndex, moduleIndex, arrayIndex, sipmIndex, property, newList, isOffset) => {
        setMeasurementData(prevMeasurementData => {
            const updatedData = prevMeasurementData;

            // If all indexes are defined, update the corresponding IVVoltages/SPSVoltages
            if (blockIndex !== undefined && moduleIndex !== undefined && arrayIndex !== undefined && sipmIndex !== undefined) {
                if (property === 'IV') {
                    updatedData.Blocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].SiPMs[sipmIndex].IVVoltages = newList.slice();
                } else if (property === 'SPS') {
                    updatedData.Blocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].SiPMs[sipmIndex].SPSVoltages = newList.slice();
                    updatedData.Blocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].SiPMs[sipmIndex].SPSVoltagesIsOffsets = isOffset;
                }
            }
            else if (blockIndex !== undefined && moduleIndex !== undefined && arrayIndex !== undefined) {
                if (property === 'IV') {
                    updatedData.Blocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].SiPMs =
                        updatedData.Blocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].SiPMs.map(sipm => ({
                            ...sipm,
                            IVVoltages: newList.slice()
                        }));
                }
            }
            else {
                // If any index is undefined, update the parent IVVoltages/SPSVoltages
                if (property === 'IV') {
                    updatedData.IVVoltages = newList.slice();
                }
                else if (property === 'SPS') {
                    updatedData.SPSVoltages = newList.slice();
                    updatedData.SPSVoltagesIsOffsets = isOffset;
                }
            }

            return updatedData;
        });
    };

    const updateSiPM = (blockIndex, moduleIndex, arrayIndex, sipmIndex, property) => {
        const updatedBlocks = [...measurementData.Blocks];
        let currentlySetProperties = areAllPropertiesSet(blockIndex, moduleIndex, arrayIndex, sipmIndex);

        console.log(currentlySetProperties);

        if (sipmIndex !== undefined) {
            // Update single SiPM
            const updatedSiPM = { ...updatedBlocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].SiPMs[sipmIndex] };

            // Toggle property
            if (property === "Both") {
                let toSet = currentlySetProperties.allSPSSet && currentlySetProperties.allIVSet;
                updatedSiPM["IV"] = toSet === false ? 1 : 0;
                updatedSiPM["SPS"] = toSet === false ? 1 : 0;
            }
            else {
                updatedSiPM[property] = updatedSiPM[property] === 0 ? 1 : 0;
            }
            

            updatedBlocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].SiPMs[sipmIndex] = updatedSiPM;
        } else if (arrayIndex !== undefined) {
            // Update all SiPMs in the array
            updatedBlocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].SiPMs.forEach(sipm => {
                if (property === "IV") {
                    sipm["SPS"] = 0;
                    sipm[property] = currentlySetProperties.allIVSet === false ? 1 : 0;
                }
                else if (property === "SPS") {
                    sipm["IV"] = 0;
                    sipm[property] = currentlySetProperties.allSPSSet === false ? 1 : 0;
                }
                else if (property === "Both") {
                    let toSet = currentlySetProperties.allSPSSet && currentlySetProperties.allIVSet;
                    sipm["IV"] = toSet === false ? 1 : 0;
                    sipm["SPS"] = toSet === false ? 1 : 0;
                }
            });
        } else if (moduleIndex !== undefined) {
            // Update all SiPMs in the module
            updatedBlocks[blockIndex].Modules[moduleIndex].Arrays.forEach(array => {
                array.SiPMs.forEach(sipm => {
                    if (property === "IV") {
                        sipm["SPS"] = 0;
                        sipm[property] = currentlySetProperties.allIVSet === false ? 1 : 0;
                    }
                    else if (property === "SPS") {
                        sipm["IV"] = 0;
                        sipm[property] = currentlySetProperties.allSPSSet === false ? 1 : 0;
                    }
                    else if (property === "Both") {
                        let toSet = currentlySetProperties.allSPSSet && currentlySetProperties.allIVSet;
                        sipm["IV"] = toSet === false ? 1 : 0;
                        sipm["SPS"] = toSet === false ? 1 : 0;
                    }
                });
            });
        } else {
            // Update all SiPMs in the block
            updatedBlocks[blockIndex].Modules.forEach(module => {
                module.Arrays.forEach(array => {
                    array.SiPMs.forEach(sipm => {
                        if (property === "IV") {
                            sipm["SPS"] = 0;
                            sipm[property] = currentlySetProperties.allIVSet === false ? 1 : 0;
                        }
                        else if (property === "SPS") {
                            sipm["IV"] = 0;
                            sipm[property] = currentlySetProperties.allSPSSet === false ? 1 : 0;
                        }
                        else if (property === "Both") {
                            let toSet = currentlySetProperties.allSPSSet && currentlySetProperties.allIVSet;
                            sipm["IV"] = toSet === false ? 1 : 0;
                            sipm["SPS"] = toSet === false ? 1 : 0;
                        }
                    });
                });
            });
        }

        setMeasurementData({ ...measurementData, Blocks: updatedBlocks });
    };

    const areAllPropertiesSet = (blockIndex, moduleIndex, arrayIndex, sipmIndex) => {
        let siPMs;
        if (sipmIndex !== undefined) {
            siPMs = measurementData.Blocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].SiPMs[sipmIndex];
            const allIVSet = siPMs.IV === 1;
            const allSPSSet = siPMs.SPS === 1;
            return { allIVSet, allSPSSet };
        }
        else if (arrayIndex !== undefined) {
            // If arrayIndex is provided, check properties for all SiPMs in the specified array
            siPMs = measurementData.Blocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].SiPMs;
        }
        else {
            // If arrayIndex is undefined, check properties for all SiPMs in the module
            siPMs = measurementData.Blocks[blockIndex].Modules[moduleIndex].Arrays.flatMap(array => array.SiPMs);
        }

        const allIVSet = siPMs.every(sipm => sipm.IV === 1);
        const allSPSSet = siPMs.every(sipm => sipm.SPS === 1);
        return { allIVSet, allSPSSet };
    };

    const updateBarcode = (blockIndex, moduleIndex, arrayIndex, newBarcode) => {
        const updatedBlocks = [...measurementData.Blocks];
        updatedBlocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].Barcode = newBarcode;

        setMeasurementData({ ...measurementData, Blocks: updatedBlocks });
    };

    const updateBarcodeTemp = (blockIndex, moduleIndex, arrayIndex, newBarcode) => {
        const updatedBlocks = [...tempMeasurementData.Blocks];
        updatedBlocks[blockIndex].Modules[moduleIndex].Arrays[arrayIndex].Barcode = newBarcode;

        setTempMeasurementData({ ...tempMeasurementData, Blocks: updatedBlocks });
    };

    const isAnyMeasurementRunning = () => {
        if (instrumentStatuses.CurrentTask === TaskTypes.Finished || instrumentStatuses.CurrentTask === TaskTypes.Idle) {
            return false;
        }
        else {
            return true;
        }
    }

    const canToggleMeasurementView = () => {
        if (instrumentStatuses.CurrentTask === TaskTypes.Finished) {
            return true;
        }
        else {
            return false;
        }
    }

    const toggleMeasurementView = () => {
        let prevValue = measurementDataView;
        if (instrumentStatuses.CurrentTask === TaskTypes.Finished) {
            setMeasurementDataView(!prevValue);
        }
    }

    const updateSelectedForMeasurementBasedOnIV = () => {
        setMeasurementStates(prevMeasurementStates => {
            // Create a deep copy of measurementStates to avoid mutation
            const updatedMeasurementStates = {
                ...prevMeasurementStates,
                Blocks: prevMeasurementStates.Blocks.map((block, blockIdx) => ({
                    ...block,
                    Modules: block.Modules.map((module, moduleIdx) => ({
                        ...module,
                        Arrays: module.Arrays.map((array, arrayIdx) => ({
                            ...array,
                            SiPMs: array.SiPMs.map((sipm, sipmIdx) => {
                                const correspondingSiPM = measurementData.Blocks[blockIdx].Modules[moduleIdx].Arrays[arrayIdx].SiPMs[sipmIdx];
                                // Check if IV is 1 in measurementData and set SelectedForMeasurement to true
                                return correspondingSiPM.IV === 1
                                    ? { ...sipm, Checks: { ...sipm.Checks, SelectedForMeasurement: true } }
                                    : sipm;
                            })
                        }))
                    }))
                }))
            };

            return updatedMeasurementStates; // Return the updated measurementStates
        });
    };


    const activeSiPMs = measurementStates.ActiveSiPMs;

    return (
        <MeasurementContext.Provider
            value={{
                measurementData, updateMeasurementData, updateSiPM, updateBarcode, areAllPropertiesSet,
                isAnyMeasurementRunning, toasts, addToast, dismissToast, updateVoltages,
                updateMeasurementStates, measurementStates, updateSiPMMeasurementState,
                instrumentStatuses, updateInstrumentStates, updateSiPMMeasurementStates,
                resetSiPMMeasurementStates, pulserState, setPulserState, updateCurrentTask, canToggleMeasurementView,
                toggleMeasurementView, measurementDataView, handleShowLogModal, handleCloseLogModal, showLogModal,
                coolerStateHandler, updateCoolerStateHandler, updateCoolerData,
                handleShowPulserLEDModal, handleClosePulserLEDModal, showPulserLEDModal, fetchCurrentRun,
                canMeasurementStart, updateActiveSiPMs, activeSiPMs, showMeasurementWizard, handleCloseMeasurementWizard,
                handleShowMeasurementWizard, updateVopData, updateVopDataTemp, updateBarcodeTemp, handleSaveBarcodeChanges,
                showExportExcelModal, handleShowExcelExportModal, handleCloseExcelExportModal, updateSelectedForMeasurementBasedOnIV
            }}>
            {children}
        </MeasurementContext.Provider>
    );
};