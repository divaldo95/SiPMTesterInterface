import { createContext, useContext, useState, useEffect } from 'react';
import SiPMSensor from './SiPMSensor';
import SiPMArray from './SiPMArray';
import SiPMModule from './SiPMModule';
import SiPMBlock from './SiPMBlock';
import { StatusEnum, getStatusBackgroundClass } from '../enums/StatusEnum';
import { MeasurementProvider } from '../context/MeasurementContext';
import ButtonsComponent from './ButtonsComponent';
import FileSelectCard from './FileSelectCard';
import ToastComponent from './ToastComponent';
import VoltageListComponent from './VoltageListComponent';

function Test() {
    const [count, setCount] = useState(0);

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
        increment();
    };

    const buttons = [
        {
            text: "Previous",
            onClick: handleButtonClick1,
            disabled: isFirstStep(), // Set to true to disable the button
            className: "bg-danger text-light"
        },
        {
            text: "Next",
            onClick: handleButtonClick2,
            disabled: false, // Set to true to disable the button
            className: "bg-success text-light"
        },
        // Add more buttons as needed
    ];

    const handleVoltageList = (newList) => {

    }

    return (
        <MeasurementProvider>
            <ToastComponent></ToastComponent>
            <div className={`${count !== 0 ? 'd-none' : ''}`}>
                <FileSelectCard className="mb-4" inputText="Measurement Settings" handleFileResult={(data) => { console.log(data); }}></FileSelectCard>
                <SiPMBlock BlockIndex={0} ModuleCount={2} ArrayCount={4} SiPMCount={16}>
                </SiPMBlock>
            </div>

            <div className={`${count !== 1 ? 'd-none' : ''}`}>
                <VoltageListComponent className="" MeasurementMode="IV" handleNewList={handleVoltageList}></VoltageListComponent>
            </div>

            
            <ButtonsComponent className="mb-2" buttons={buttons}></ButtonsComponent>
            
        </MeasurementProvider>

        //<SiPMModule block={0} module={0} arrayCount={4} sipmCount={16}>
        //</SiPMModule >

        //<SiPMArray block={0} module={0} array={0} sipmCount={16}>
        //</SiPMArray>
        
        //<SiPMSensor block={0} module={0} array={0} sipm={0} backgroundColor={StatusEnum.MeasurementOK} onClickHandler={(block, module, array, sipm) => { alert("Clicked") }}>
        //</SiPMSensor>
    );
}

export default Test;
