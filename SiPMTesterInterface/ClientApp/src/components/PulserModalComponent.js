import { useMemo, useEffect, useState } from 'react';
import { Modal, Button, Spinner } from 'react-bootstrap';
import { Chart } from 'react-charts';
import MeasurementStateService from '../services/MeasurementStateService';
import CoolerSettingsComponent from './CoolerSettingsComponent';
import TemperaturesTable from './TemperaturesTable';

function PulserModalComponent(props) {
    const { showModal, closeModal } = props;
    const [isLoading, setIsLoading] = useState(true);
    const [isIntervalWaitingUpdate, setIsIntervalWaitingUpdate] = useState(false);
    const [isIntervalUpdateSuccess, setIsIntervalUpdateSuccess] = useState(false);
    const [isIntervalUpdateError, setIsIntervalUpdateError] = useState(false);

    const [pulserFormData, setPulserFormData] = useState({
        PulserConnected: false,
        PulserReadingInterval: 0,
        Temperatures: [],
        CoolerStates: []
    });

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

        MeasurementStateService.setPulser(pulserFormData.PulserReadingInterval)
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
        refreshPulserState();
    }, []); // Empty dependency array ensures the effect runs only once when the component mounts

    const refreshPulserState = async () => {
        setIsLoading(true);
        try {
            const data = await MeasurementStateService.getPulserStateDetails()
                .then((resp) => {
                    setIsLoading(false);
                    console.log(resp);
                    setPulserFormData(prevFormData => ({
                        ...prevFormData,
                        PulserConnected: resp.PulserConnected,
                        PulserReadingInterval: resp.PulserReadingInterval,
                        Temperatures: resp.Temperatures,
                        CoolerStates: resp.CoolerStates
                    }));
                })

        } catch (error) {
            setIsLoading(false);
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
                <form onSubmit={handlePulserSubmit} className="needs-validation">
                    <div className="input-group mb-3">
                        <input type="number" className="form-control" placeholder="Pulser readout interval in seconds (int)"
                            aria-label="Readout interval" aria-describedby="pulser-submit-btn"
                            value={pulserFormData.PulserReadingInterval} onChange={handlePulserChange}
                            name="PulserReadingInterval"
                            min="0"
                            max="60"
                        >
                        </input>
                        <button
                            className={`btn ${isIntervalUpdateSuccess ? "btn-success" : isIntervalUpdateError ? "btn-danger" : "btn-primary"}`}
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
                        </button>
                    </div>
                </form>

                <CoolerSettingsComponent></CoolerSettingsComponent>

                <TemperaturesTable temperaturesArray={pulserFormData.Temperatures}>
                </TemperaturesTable>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={closeModal}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
}

export default PulserModalComponent;