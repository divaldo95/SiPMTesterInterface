import { useState, useEffect } from 'react';
import { Modal, Button, Table } from 'react-bootstrap';
import MeasurementStateService from '../services/MeasurementStateService';

import RootTGraphComponent from './RootTGraphComponent';

function SiPMMeasurementModal(props) {
    const { showModal, closeModal, BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, BreakdownVoltage, CompensatedBreakdownVoltage, Chi2 } = props;
    const [ error, setError ] = useState();
    const [isLoading, setIsLoading] = useState(true);
    const [x, setX] = useState([]);
    const [y, setY] = useState([]);
    const [renderGraph, setRenderGraph] = useState(false);

    const x_test = Array.from({ length: 1000 }, (_, i) => i);
    const y_test = Array.from({ length: 1000 }, () => Math.floor(Math.random() * 1000));

    const fetchData = async () => {
        //setIsLoading(true);
        try {
            const data = await MeasurementStateService.getSiPMMeasurementData(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex);
            console.log(data);
            setX(data.IVResult.DMMVoltage);
            setY(data.IVResult.SMUCurrent);
            console.log(x);
            console.log(y);
        } catch (error) {
            // Handle the error if needed
            setError(error);
            //console.log(error);
        }
        setIsLoading(false);
    };

    useEffect(() => {
        
    }, [BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex, isLoading]);

    const handleModalOpen = (blockIndex, moduleIndex, arrayIndex, sipmIndex) => {
        fetchData();
        setRenderGraph(true);
    };

    const handleModalClose = () => {
        setRenderGraph(false);
        closeModal();
    }

    return (
        <Modal show={showModal} onHide={closeModal} onShow={() => handleModalOpen(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex)} centered size="lg" fullscreen={false}>
            <Modal.Header closeButton>
                <Modal.Title>{`SiPM Measurement (Block: ${BlockIndex} Module: ${ModuleIndex} Array: ${ArrayIndex} SiPM: ${SiPMIndex})`}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <div className="text-center d-block d-inline-block">
                    <div className="row">
                        <div className="col">
                            {isLoading ? (
                                <div className="spinner-border" role="status">
                                    <span className="visually-hidden">Loading...</span>
                                </div>
                            ) : (
                                    <>
                                        {error != undefined ? (
                                            <div className="container border border-danger bg-danger text-white">
                                                <div className="p-3 ">
                                                    {error.response ? error.response.data.Error : error.message}
                                                </div>
                                            </div>
                                        ) : (
                                            <>
                                                <RootTGraphComponent render={renderGraph} x={x} y={y}>
                                                </RootTGraphComponent>
                                                <Table bordered>
                                                    <thead>
                                                        <tr>
                                                            <th>Breakdown Voltage</th>
                                                            <th>Compensated Breakdown Voltage</th>
                                                            <th>Chi2</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                        <tr key={0}>
                                                            <td>{BreakdownVoltage}</td>
                                                            <td>{CompensatedBreakdownVoltage}</td>
                                                            <td>{Chi2}</td>
                                                        </tr>
                                                    </tbody>
                                                </Table>
                                            </> 
                                        )}
                                    </>
                            )}
                            
                        </div>
                    </div>
                </div>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={closeModal}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
}

export default SiPMMeasurementModal;
