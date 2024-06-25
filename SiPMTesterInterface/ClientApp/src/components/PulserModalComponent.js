import { useMemo, useEffect, useState, useContext } from 'react';
import { Modal, Button, Spinner, Form, InputGroup, FloatingLabel } from 'react-bootstrap';
import { Chart } from 'react-charts';
import MeasurementStateService from '../services/MeasurementStateService';
import CoolerSettingsComponent from './CoolerSettingsComponent';
import TemperaturesTable from './TemperaturesTable';
import CoolerTable from './CoolerTable';
import { MeasurementContext } from '../context/MeasurementContext';


function PulserModalComponent(props) {
    const { showModal, closeModal } = props;
    const [isLoading, setIsLoading] = useState(true);
    const [isIntervalWaitingUpdate, setIsIntervalWaitingUpdate] = useState(false);
    const [isIntervalUpdateSuccess, setIsIntervalUpdateSuccess] = useState(false);
    const [isIntervalUpdateError, setIsIntervalUpdateError] = useState(false);

    const [pulserFormData, setPulserFormData] = useState({
        PulserState: false,
        TotalSeconds: 0,
        Temperatures: [],
        CoolerStates: []
    });

    const { coolerStateHandler, updateCoolerData, updateCoolerStateHandler } = useContext(MeasurementContext);

    const [validated, setValidated] = useState(false);

    const onSubmit = (event) => {
        const form = event.currentTarget;
        if (form.checkValidity() === false) {
            event.preventDefault();
            event.stopPropagation();
        } else {
            handlePulserSubmit(event);
        }
        setValidated(true);
    };

    const handlePulserChange = (e) => {
        const { name, value, type, checked } = e.target;
        setPulserFormData(prevFormData => ({
            ...prevFormData,
            [name]: type === 'checkbox' ? checked : value //not really needed
        }));
    };

    const handlePulserSubmit = (e) => {
        e.preventDefault();
        setIsIntervalWaitingUpdate(true);

        MeasurementStateService.setPulser(pulserFormData.TotalSeconds)
            .then((resp) => {
                setIsIntervalUpdateSuccess(true);
                setIsIntervalUpdateError(false);
                setIsIntervalWaitingUpdate(false);

                setTimeout(() => {
                    setIsIntervalUpdateSuccess(false);
                    setIsIntervalUpdateError(false);
                }, 3000);
            }).catch((err) => {
                //alert("Pulser not set")
                setIsIntervalUpdateSuccess(false);
                setIsIntervalUpdateError(true);
                setIsIntervalWaitingUpdate(false);

                setTimeout(() => {
                    setIsIntervalUpdateSuccess(false);
                    setIsIntervalUpdateError(false);
                }, 3000);
            });

        // Handle form submission logic here
        
        console.log("Submitted pulser");
    };

    useEffect(() => {
        refreshAll();
    }, []); // Empty dependency array ensures the effect runs only once when the component mounts

    const refreshAll = () => {
        refreshPulserState();
        refreshCoolerStates();
    }

    const refreshPulserState = async () => {
        setIsLoading(true);
        try {
            const data = await MeasurementStateService.getPulserStateDetails()
                .then((resp) => {
                    setIsLoading(false);
                    console.log(resp);
                    setPulserFormData(prevFormData => ({
                        ...prevFormData,
                        PulserState: resp.PulserState,
                        TotalSeconds: resp.TotalSeconds,
                        Temperatures: resp.Temperatures,
                        CoolerStates: resp.CoolerStates
                    }));
                })

        } catch (error) {
            setIsLoading(false);
        }
    };

    const refreshCoolerStates = async () => {
        try {
            const data = await MeasurementStateService.getAllCooler()
                .then((resp) => {
                    console.log(resp);
                    updateCoolerStateHandler(resp);
                })

        } catch (error) {
            console.log(error);
        }
    };

    const data = [
        {
            label: 'Series 1',
            data: [
                {
                    primary: '2022-02-03T00:00:00.000Z',
                    likes: 130,
                },
                {
                    primary: '2022-03-03T00:00:00.000Z',
                    likes: 150,
                },
            ],
        },
        {
            label: 'Series 2',
            data: [
                {
                    primary: '2022-04-03T00:00:00.000Z',
                    likes: 200,
                },
                {
                    primary: '2022-05-03T00:00:00.000Z',
                    likes: 250,
                },
            ],
        },
    ]

    const primaryAxis = useMemo(
        (): AxisOptions<MyDatum> => ({
            getValue: datum => datum.primary,
        }),
        []
    )

    const secondaryAxes = useMemo(
        (): AxisOptions<MyDatum>[] => [
            {
                getValue: datum => datum.likes,
            },
        ],
        []
    )

    return (
        <Modal show={showModal} onHide={closeModal} centered size="lg" fullscreen={false} scrollable={true}>
            <Modal.Header closeButton>
                <Modal.Title>Pulser details</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Form noValidate validated={validated} onSubmit={onSubmit} className="needs-validation">
                    <InputGroup className="mb-3">
                        <FloatingLabel controlId="floatingPulserInput" label="Pulser readout interval in seconds (int)">
                            <Form.Control
                                type="number"
                                placeholder="Pulser readout interval in seconds"
                                aria-label="Readout interval"
                                aria-describedby="pulser-submit-btn"
                                value={pulserFormData.TotalSeconds}
                                onChange={handlePulserChange}
                                name="TotalSeconds"
                                min="0"
                                max="60"
                                required
                                isInvalid={validated && (pulserFormData.TotalSeconds < 0 || pulserFormData.TotalSeconds > 60)}
                            />
                            <Form.Control.Feedback type="invalid">
                                Please enter a value between 0 and 60.
                            </Form.Control.Feedback>
                        </FloatingLabel>
                        <Button
                            className={isIntervalUpdateSuccess ? "btn-success" : isIntervalUpdateError ? "btn-danger" : "btn-primary"}
                            type="submit"
                            id="pulser-submit-btn"
                            disabled={isIntervalWaitingUpdate || isIntervalUpdateSuccess || isIntervalUpdateError}
                        >
                            {isIntervalWaitingUpdate ? (
                                <>
                                    Applying pulser...
                                    <Spinner animation="border" role="status" size="sm" className="ms-2">
                                        <span className="visually-hidden">Loading...</span>
                                    </Spinner>
                                </>
                            ) : (
                                isIntervalUpdateSuccess ? "Pulser applied" : isIntervalUpdateError ? "Error applying pulser" : "Apply pulser"
                            )}
                        </Button>
                    </InputGroup>
                </Form>

                <CoolerSettingsComponent coolerData={coolerStateHandler} updateCoolerData={updateCoolerData}></CoolerSettingsComponent>

                <TemperaturesTable temperaturesArray={pulserFormData.Temperatures}>
                </TemperaturesTable>

                <CoolerTable coolerArray={pulserFormData.CoolerStates}>
                </CoolerTable>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="primary" onClick={refreshAll}>
                    Refresh data
                </Button>
                <Button variant="secondary" onClick={closeModal}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
}

export default PulserModalComponent;