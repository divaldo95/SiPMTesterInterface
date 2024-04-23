import React from 'react';
import { Modal, Button } from 'react-bootstrap';
import { Chart } from 'react-charts';

function PulserModalComponent(props) {
    const { showModal, closeModal } = props;

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

    const primaryAxis = React.useMemo(
        (): AxisOptions<MyDatum> => ({
            getValue: datum => datum.primary,
        }),
        []
    )

    const secondaryAxes = React.useMemo(
        (): AxisOptions<MyDatum>[] => [
            {
                getValue: datum => datum.likes,
            },
        ],
        []
    )

    return (
        <Modal show={showModal} onHide={closeModal} centered size="lg" fullscreen={false}>
            <Modal.Header closeButton>
                <Modal.Title>Pulser details</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <div style={{ height: '400px' }}>
                    <Chart
                        initialWidth={400}
                        initialHeight={500}
                        options={{
                            data,
                            primaryAxis,
                            secondaryAxes,
                        }}
                    />
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

export default PulserModalComponent;