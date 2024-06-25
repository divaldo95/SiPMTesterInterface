import { createContext, useContext, useState, useEffect } from 'react';
import SiPMSensor from './SiPMSensor';
import SiPMArray from './SiPMArray';
import SiPMModule from './SiPMModule';
import SiPMBlock from './SiPMBlock';
import { StatusEnum, MeasurementStateEnum, getStatusBackgroundClass } from '../enums/StatusEnum';
import { MeasurementContext } from '../context/MeasurementContext';
import { LogContext } from '../context/LogContext';
import ButtonsComponent from './ButtonsComponent';
import FileSelectCard from './FileSelectCard';
import OneButtonCard from './OneButtonCard';
import ToastComponent from './ToastComponent';
import VoltageListComponent from './VoltageListComponent';
import MeasurementStateService from '../services/MeasurementStateService';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { MessageTypeEnum, getIconClass } from '../enums/MessageTypeEnum';
import { LogMessageType } from '../enums/LogMessageTypeEnum';
import StatesCard from './StatesCard';
import SerialStateCard from './SerialStateCard';
import LogModal from './LogModal';
import ErrorMessageModal from './ErrorMessageModal';
import MeasurementSidebar from './MeasurementSidebar';
import { Container, Row, Col } from 'react-bootstrap';
import PulserValuesModal from './PulserValuesModal';

function Measurement() {
    const [count, setCount] = useState(0);
    const [sidebarCollapsed, setSidebarCollapsed] = useState(true);
    const { measurementData, addToast, isAnyMeasurementRunning, updateVoltages,
        updateInstrumentStates, instrumentStatuses, updateSiPMMeasurementStates,
        resetSiPMMeasurementStates, pulserState, setPulserState, updateCurrentTask,
        measurementDataView, showLogModal, handleCloseLogModal,
        handleClosePulserLEDModal, showPulserLEDModal } = useContext(MeasurementContext);

    
    const { logs, fetchLogs, updateLogsResolved, unresolvedLogs, appendLog, unresolvedLogCount, currentError } = useContext(LogContext);
    const [showErrorsDialog, setShowErrorsDialog] = useState(false);    

    useEffect(() => {
        console.log("Unresolved count changed to " + unresolvedLogCount);
        if (unresolvedLogCount > 0) {
            setShowErrorsDialog(true);
        } else {
            setShowErrorsDialog(false);
        }
    }, [unresolvedLogCount]);

    const handleErrorMessageBtnClick = async (errorId, buttonType) => {
        try {
            const response = MeasurementStateService.setLogResponse(errorId, buttonType)
                .then((resp) => {
                    console.log("Updated log message state on server");
                    updateLogsResolved(errorId, buttonType);
            })
            // Handle response as needed
        } catch (error) {
            console.error('Error sending data to backend:', error);
        }
    };

    const handleErrorDialogClose = () => {
        setShowErrorsDialog(false);
    };

    const updateIvAnalysationResultState = (blockIndex, moduleIndex, arrayIndex, sipmIndex, newData) => {
        updateSiPMMeasurementStates(blockIndex, moduleIndex, arrayIndex, sipmIndex, "IVAnalysationResult", newData.IVAnalysationResult);
        updateSiPMMeasurementStates(blockIndex, moduleIndex, arrayIndex, sipmIndex, "IVTimes", newData.IVTimes);
    };

    const updateIvConnectionState = (newValue) => {
        updateInstrumentStates(prevStatus => ({
            ...prevStatus,
            IVConnectionState: newValue
        }));
    };

    const updateSpsConnectionState = (newValue) => {
        updateInstrumentStates(prevStatus => ({
            ...prevStatus,
            SPSConnectionState: newValue
        }));
    };

    const updateIvState = (newValue) => {
        updateInstrumentStates(prevStatus => ({
            ...prevStatus,
            IVState: newValue
        }));
    };

    const updateSpsState = (newValue) => {
        updateInstrumentStates(prevStatus => ({
            ...prevStatus,
            SPSState: newValue
        }));
    };

    const isFirstStep = () => {
        return count === 0;
    }

    const isLastStep = () => {
        return count === 1;
    }

    const increment = () => {
        if (count < 3) {
            setCount(prevCount => prevCount + 1);
        }
    };

    const decrement = () => {
        if (count > 0) {
            setCount(prevCount => prevCount - 1);
        }
    };


    const handleButtonClick1 = () => {
        decrement();
    };

    const stopMeasurement = () => {
        try {
            MeasurementStateService.stopMeasurement();
        } catch (error) {
            console.log("Failed to stop measurement: " + error);
        }
    }

    const handleButtonClick2 = () => {
        //increment();
        MeasurementStateService.startMeasurement(measurementData);
        resetSiPMMeasurementStates();
        //updateIVMeasurementIsRunning(true);
        
    };

    const refreshMeasuredSiPMStates = async () => {
        try {
            const data = await MeasurementStateService.getMeasuredSiPMStates()
                .then((resp) => {
                    updateSiPMMeasurementStates(undefined, undefined, undefined, undefined, undefined, resp);
                    console.log(resp);
                })
        } catch (error) {
            //addToast(MessageTypeEnum.Error, JSON.stringify(error));
        }
    }

    const refreshMeasurementState = async () => {
        //setIsLoading(true);
        try {
            const data = await MeasurementStateService.getMeasurementStates()
                .then((resp) => {
                    updateInstrumentStates(resp);
                    if (isAnyMeasurementRunning() === false) {
                        refreshMeasuredSiPMStates();
                    }
                    else {
                        //console.log("sgfa");
                    }
            })
            
        } catch (error) {
            // Handle the error if needed
            addToast(MessageTypeEnum.Error, JSON.stringify(error));
            //console.log(error);
        }
    };

    const refreshPulserConnectionState = async () => {
        //setIsLoading(true);
        try {
            const data = await MeasurementStateService.getPulserState()
                .then((resp) => {
                    setPulserState(resp.PulserState);
                    //console.log(resp);
                })
        } catch (error) {
            addToast(MessageTypeEnum.Error, JSON.stringify(error));
        }
    };

    const getMeasurementTimes = async () => {
        try {
            const data = await MeasurementStateService.getMeasurementTimes();
            addToast(MessageTypeEnum.Debug, JSON.stringify(data));
        } catch (error) {
            // Handle the error if needed
            addToast(MessageTypeEnum.Debug, JSON.stringify(error));
            //console.log(error);
        }
    };

    useEffect(() => {
        fetchLogs();

        const connection = new HubConnectionBuilder()
            .withUrl('hub')
            .withAutomaticReconnect()
            .build();
        connection.start()
            .then(result => {
                //setTestState("Connected");

                connection.on('ReceiveGlobalStateChange', (globalStateModel) => {
                    // Handle received global state change
                    console.log('Received global state change:', globalStateModel);
                    //addToast(MessageTypeEnum.Debug, JSON.stringify(globalStateModel));
                });

                connection.on('ReceiveIVMeasurementStateChange', (ivModel) => {
                    // Handle received IV measurement state change
                    //setTestState(ivModel);
                    console.log('Received IV measurement state change:', ivModel);
                    updateIvState(ivModel);
                    //addToast(MessageTypeEnum.Debug, JSON.stringify(ivModel));
                });

                connection.on('ReceiveSPSMeasurementStateChange', (spsModel) => {
                    // Handle received SPS measurement state change
                    console.log('Received SPS measurement state change:', spsModel);
                    updateSpsState(spsModel);
                });

                connection.on('ReceiveSiPMIVMeasurementDataUpdate', (currentSiPMModel) => {
                    // Handle received SPS measurement state change
                    console.log('Received SiPM IV measurement:', currentSiPMModel);
                    updateSiPMMeasurementStates(currentSiPMModel.Block, currentSiPMModel.Module, currentSiPMModel.Array, currentSiPMModel.SiPM, "IVMeasurementDone", true);
                    addToast(MessageTypeEnum.Information,`SiPM(${currentSiPMModel.Block}, ${currentSiPMModel.Module}, ${currentSiPMModel.Array}, ${currentSiPMModel.SiPM}) IV data received`);
                });
                

                connection.on('ReceiveIVConnectionStateChange', (ivConn) => {
                    // Handle received SPS measurement state change
                    //console.log(ivConn);
                    updateIvConnectionState(ivConn);
                    //addToast(MessageTypeEnum.Debug, 'Received IV connection state change:', ivConn);
                });

                connection.on('ReceiveIVAnalysationResult', (currentSiPM, ivARes) => {
                    console.log("Reveived IV analysation result:");
                    console.log(currentSiPM);
                    console.log(ivARes);
                    updateIvAnalysationResultState(currentSiPM.Block, currentSiPM.Module, currentSiPM.Array, currentSiPM.SiPM, ivARes);
                    addToast(MessageTypeEnum.Debug, 'Received IV analysation data: ' + JSON.stringify(ivARes.IVAnalysationResult));
                });

                connection.on('ReceiveLogMessage', (logData) => {
                    if (!logData.NeedsAttention && !(logData.MessageType === LogMessageType.Error || logData.MessageType === LogMessageType.Fatal)) {
                        addToast(MessageTypeEnum.Info, logData.Message);
                    }
                    appendLog(logData);
                });

                connection.on('ReceiveCurrentTask', (currentTask) => {
                    updateCurrentTask(currentTask);
                });
            })

            .catch(e => {
                console.log('Connection failed: ', e);
            });

        refreshMeasurementState();
        refreshPulserConnectionState();
    }, []); // Empty dependency array ensures the effect runs only once when the component mounts

    const toggleSidebarCollapsed = () => {
        if (sidebarCollapsed) {
            setSidebarCollapsed(false);
        }
        else {
            setSidebarCollapsed(true);
        }
        //updateIVMeasurementIsRunning(!currentState);
    }

    const buttons = [
        {
            text: "Previous",
            onClick: handleButtonClick1,
            disabled: isFirstStep(), // Set to true to disable the button
            className: "bg-danger text-light"
        },
        {
            text: "Get Times",
            onClick: getMeasurementTimes,
            disabled: false, // Set to true to disable the button
            className: "bg-primary text-light"
        },
        {
            text: "Start",
            onClick: handleButtonClick2,
            disabled: isAnyMeasurementRunning(), // Set to true to disable the button
            className: "bg-success text-light"
        },
        // Add more buttons as needed
    ];

    const handleVoltageList = (newList) => {
        //Update global IV voltage list
        updateVoltages(undefined, undefined, undefined, undefined, "IV", newList, false);
        //console.log(measurementData);
    };

    const sidebarMargin = () => {
        return '0px';
        if(sidebarCollapsed) {
            return '80px';
        }
        else {
            return '80px';
        }
    }

    return (
        <>
            <div style={{ display: 'flex' }}>
                { /* <MeasurementSidebar collapsed={sidebarCollapsed} toggleCollapsed={toggleSidebarCollapsed} openErrorsModal={handleShowLogModal}></MeasurementSidebar> */ }
                <div className="m-3" style={{ marginLeft: `${sidebarMargin()}`, paddingLeft: `${sidebarMargin()}`, width: '100%' }}>
                    <ToastComponent></ToastComponent>
                    <LogModal show={showLogModal} handleClose={handleCloseLogModal}></LogModal>
                    <ErrorMessageModal show={showErrorsDialog} error={currentError} handleClose={handleErrorDialogClose} handleButtonClick={handleErrorMessageBtnClick}></ErrorMessageModal>
                    <PulserValuesModal show={showPulserLEDModal} handleClose={handleClosePulserLEDModal}></PulserValuesModal>
                    <div className={`${count !== 0 ? 'd-none' : ''}`}>
                        <div className="row mb-4">
                            <div className="col">
                                <FileSelectCard className="h-100" inputText="Measurement Settings" handleFileResult={(data) => { console.log(data); }}></FileSelectCard>
                            </div>
                            <div className="col">
                                <OneButtonCard className="h-100" title="STOP MEASUREMENT" buttonText="STOP" buttonColor="bg-danger" textColor="text-white"
                                    clickFunction={stopMeasurement} tooltipMessage="Stop all running measurements" buttonIcon="bi-sign-stop">
                                </OneButtonCard>
                            </div>
                            <div className="col">
                                <OneButtonCard className="h-100" title="Export settings" buttonText="Export" buttonColor="bg-secondary" textColor="text-white"
                                    clickFunction={() => { console.log("Clicked export button") }} tooltipMessage="Export current settings" buttonIcon="bi-download">
                                </OneButtonCard>
                            </div>
                        </div>

                        <div className="row mb-4">
                            <div className="col">
                                <StatesCard className="h-100" MeasurementType="IV" ConnectionState={instrumentStatuses.IVConnectionState} MeasurementState={instrumentStatuses.IVState}></StatesCard>
                            </div>
                            <div className="col">
                                <StatesCard className="h-100" MeasurementType="SPS" ConnectionState={instrumentStatuses.SPSConnectionState} MeasurementState={instrumentStatuses.SPSState}></StatesCard>
                            </div>
                            <div className="col">
                                <SerialStateCard className="h-100" SerialInstrumentName="Pulser" ConnectionState={pulserState}></SerialStateCard>
                            </div>
                        </div>

                        <SiPMBlock BlockIndex={0} ModuleCount={2} ArrayCount={4} SiPMCount={16}>
                        </SiPMBlock>
                    </div>

                    <div className={`${measurementDataView === true ? 'd-none' : ''}`}>
                        <VoltageListComponent className="" MeasurementMode="IV" handleNewList={handleVoltageList}></VoltageListComponent>
                    </div>


                    <ButtonsComponent className="mb-2" buttons={buttons}></ButtonsComponent>
                </div>
            </div>
            
        </>
    );
}

export default Measurement;
