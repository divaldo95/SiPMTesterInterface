﻿import { createContext, useContext, useState, useEffect } from 'react';
import SiPMSensor from './SiPMSensor';
import SiPMArray from './SiPMArray';
import SiPMModule from './SiPMModule';
import SiPMBlock from './SiPMBlock';
import { StatusEnum, MeasurementStateEnum, getStatusBackgroundClass } from '../enums/StatusEnum';
import {  MeasurementContext } from '../context/MeasurementContext';
import ButtonsComponent from './ButtonsComponent';
import FileSelectCard from './FileSelectCard';
import ToastComponent from './ToastComponent';
import VoltageListComponent from './VoltageListComponent';
import MeasurementStateService from '../services/MeasurementStateService';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { MessageTypeEnum, getIconClass } from '../enums/MessageTypeEnum';
import StatesCard from './StatesCard';

function Test() {
    const [count, setCount] = useState(0);
    const { measurementData, addToast, isAnyMeasurementRunning, updateIVMeasurementIsRunning } = useContext(MeasurementContext);

    const [status, setStatus] = useState({
        ivConnectionState: 0,
        spsConnectionState: 0,
        ivState: 0,
        spsState: 0
    });

    const updateIvConnectionState = (newValue) => {
        setStatus(prevStatus => ({
            ...prevStatus,
            ivConnectionState: newValue
        }));
    };

    const updateSpsConnectionState = (newValue) => {
        setStatus(prevStatus => ({
            ...prevStatus,
            spsConnectionState: newValue
        }));
    };

    const updateIvState = (newValue) => {
        setStatus(prevStatus => ({
            ...prevStatus,
            ivState: newValue
        }));
    };

    const updateSpsState = (newValue) => {
        setStatus(prevStatus => ({
            ...prevStatus,
            spsState: newValue
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

    const handleButtonClick2 = () => {
        //increment();
        MeasurementStateService.startMeasurement(measurementData);
        updateIVMeasurementIsRunning(true);
        
    };

    const toggleMeasurementState = () => {
        const currentState = isAnyMeasurementRunning();
        updateIVMeasurementIsRunning(!currentState);
    }

    const refreshMeasurementState = async () => {
        //setIsLoading(true);
        try {
            const data = await MeasurementStateService.getMeasurementStates()
                .then(() => {
                    updateIvConnectionState(data.ivConnectionState);
                    updateIvState(data.ivState);
                    updateSpsConnectionState(data.spsConnectionState);
                    updateSpsState(data.spsState);
                    //addToast(MessageTypeEnum.Debug, JSON.stringify(data));
            })
            
            /*
            if (data.IVState === 1) {
                updateIVMeasurementIsRunning(true);
            }
            else {
                updateIVMeasurementIsRunning(false);
            }
            */
            
        } catch (error) {
            // Handle the error if needed
            //addToast(MessageTypeEnum.Debug, JSON.stringify(error));
            //console.log(error);
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
                    //addToast(MessageTypeEnum.Debug, JSON.stringify(ivModel));
                });

                connection.on('ReceiveSPSMeasurementStateChange', (spsModel) => {
                    // Handle received SPS measurement state change
                    console.log('Received SPS measurement state change:', spsModel);
                });

                connection.on('ReceiveSiPMIVMeasurementDataUpdate', (currentSiPMModel) => {
                    // Handle received SPS measurement state change
                    console.log('Received SiPM IV measurement:', currentSiPMModel);
                    addToast(MessageTypeEnum.Information,`SiPM(${currentSiPMModel.block}, ${currentSiPMModel.module}, ${currentSiPMModel.array}, ${currentSiPMModel.siPM}) IV data received`);
                });
                

                connection.on('ReceiveIVConnectionStateChange', (ivConn) => {
                    // Handle received SPS measurement state change
                    console.log(ivConn);
                    updateIvConnectionState(ivConn);
                    addToast(MessageTypeEnum.Debug, 'Received IV connection state change:', ivConn);
                });

                
            })

            .catch(e => {
                console.log('Connection failed: ', e);
            });

        refreshMeasurementState();
    }, [count]); // Empty dependency array ensures the effect runs only once when the component mounts
    

    const buttons = [
        {
            text: "Previous",
            onClick: handleButtonClick1,
            disabled: isFirstStep(), // Set to true to disable the button
            className: "bg-danger text-light"
        },
        {
            text: "Refresh state",
            onClick: refreshMeasurementState,
            disabled: false, // Set to true to disable the button
            className: "bg-primary text-light"
        },
        {
            text: "Toggle state",
            onClick: toggleMeasurementState,
            disabled: false, // Set to true to disable the button
            className: "bg-primary text-light"
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

    }

    return (
        <>
            <ToastComponent></ToastComponent>
            <div className={`${count !== 0 ? 'd-none' : ''}`}>
                <div className="row mb-4">
                    <div className="col">
                        <FileSelectCard className="h-100" inputText="Measurement Settings" handleFileResult={(data) => { console.log(data); }}></FileSelectCard>
                    </div>
                    <div className="col">
                        <StatesCard className="h-100" MeasurementType="IV" ConnectionState={status.ivConnectionState} MeasurementState={status.ivState}></StatesCard>
                    </div>
                    <div className="col">
                        <StatesCard className="h-100" MeasurementType="SPS" ConnectionState={status.spsConnectionState} MeasurementState={status.spsState}></StatesCard>
                    </div>
                </div>
                
                <SiPMBlock BlockIndex={0} ModuleCount={2} ArrayCount={4} SiPMCount={16}>
                </SiPMBlock>
            </div>

            <div className={`${count !== 0 ? 'd-none' : ''}`}>
                <VoltageListComponent className="" MeasurementMode="IV" handleNewList={handleVoltageList}></VoltageListComponent>
            </div>

            
            <ButtonsComponent className="mb-2" buttons={buttons}></ButtonsComponent>
            
        </>

        //<SiPMModule block={0} module={0} arrayCount={4} sipmCount={16}>
        //</SiPMModule >

        //<SiPMArray block={0} module={0} array={0} sipmCount={16}>
        //</SiPMArray>
        
        //<SiPMSensor block={0} module={0} array={0} sipm={0} backgroundColor={StatusEnum.MeasurementOK} onClickHandler={(block, module, array, sipm) => { alert("Clicked") }}>
        //</SiPMSensor>
    );
}

export default Test;
