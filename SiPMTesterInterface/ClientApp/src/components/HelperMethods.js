export function GetSiPMNumber(block, module, array, sipm) {
    return block * 128 + module * 64 + array * 16 + sipm;
}

export function GetSiPMLocation(sensorIndex) {
    const sensorsPerArray = 16;
    const arraysPerModule = 4;
    const modulesPerBlock = 2;

    // Calculate the indices
    const blockIndex = Math.floor(sensorIndex / (modulesPerBlock * arraysPerModule * sensorsPerArray));
    const moduleIndex = Math.floor((sensorIndex % (modulesPerBlock * arraysPerModule * sensorsPerArray)) / (arraysPerModule * sensorsPerArray));
    const arrayIndex = Math.floor((sensorIndex % (arraysPerModule * sensorsPerArray)) / sensorsPerArray);
    const sipmNumber = sensorIndex % sensorsPerArray;

    return {
        block: blockIndex,
        module: moduleIndex,
        array: arrayIndex,
        sipm: sipmNumber
    };
}
