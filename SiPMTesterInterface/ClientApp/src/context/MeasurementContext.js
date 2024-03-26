import React, { createContext, useState } from 'react';


export const MeasurementContext = createContext();

const initialState = {
    Blocks: Array.from({ length: 2 }, () => ({
        Modules: Array.from({ length: 2 }, () => ({
            Arrays: Array.from({ length: 4 }, () => ({
                SiPMs: Array.from({ length: 16 }, () => ({
                    DMMResistance: 0,
                    IV: 1,
                    IVVoltages: [],
                    SPS: 0,
                    SPSVoltages: [],
                    SPSVoltagesIsOffsets: 0
                })),
                Barcode: ""
            }))
        }))
    })),
    DMMResistance: {
        CorrectionPercentage: 10,
        Iterations: 3,
        Voltage: 30.0
    },
    IVVoltages: [],
    SPSVoltages: [],
    MeasureDMMResistance: true
};

const initialToasts = [
];

export const MeasurementProvider = ({ children }) => {
    const [measurementData, setMeasurementData] = useState(initialState);
    const [isIVMeasurementRunning, setIsIVMeasurementRunning] = useState(true);
    const [isSPSMeasurementRunning, setIsSPSMeasurementRunning] = useState(false);
    const [messages, setMessages] = useState(initialToasts);

    // Function to set the "Dismissed" property of a message based on its index
    const setDismissed = (index, dismissed) => {
        setMessages(prevMessages => {
            const updatedMessages = [...prevMessages];
            if (index >= 0 && index < updatedMessages.length) {
                updatedMessages[index].Dismissed = dismissed;
            }
            return updatedMessages;
        });
    };

    const addToast = (messageType, messageText) => {
        setMessages(prevMessages => [
            ...prevMessages,
            {
                MessageType: messageType,
                Message: messageText,
                Dismissed: false
            }
        ]);
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
            } else {
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

    const isAnyMeasurementRunning = () => {
        return isIVMeasurementRunning || isSPSMeasurementRunning; //add variables and related stuff
    }

    return (
        <MeasurementContext.Provider value={{ measurementData, updateMeasurementData, updateSiPM, updateBarcode, areAllPropertiesSet, isAnyMeasurementRunning, messages, setDismissed, addToast, updateVoltages }}>
            {children}
        </MeasurementContext.Provider>
    );
};