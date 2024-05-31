import React, { useState, useEffect } from 'react';
import { Table, Button, Collapse, Container } from 'react-bootstrap';

function TemperaturesTable(props) {
    const { className, temperaturesArray } = props;
    const [isTableVisible, setIsTableVisible] = useState(true);

    const toggleTableVisibility = () => {
        setIsTableVisible(!isTableVisible);
    };

    return (
        <Container className="mt-5">
            <div className="d-flex justify-content-between align-items-center mb-3">
                <h2>Temperatures Data</h2>
                <Button variant="link" onClick={toggleTableVisibility}>
                    {isTableVisible ? <i class="bi bi-caret-down"></i> : <i class="bi bi-caret-up"></i>}
                </Button>
            </div>
            <Collapse in={isTableVisible}>
                <div>
                    <Table bordered>
                        <thead>
                            <tr>
                                <th>Module 1</th>
                                <th>Module 2</th>
                                <th>Pulser</th>
                                <th>Control Temperature</th>
                                <th>Timestamp</th>
                            </tr>
                        </thead>
                        <tbody>
                            {temperaturesArray.map((item, index) => (
                                <tr key={index}>
                                    <td>{item.Module1.join(', ')}</td>
                                    <td>{item.Module2.join(', ')}</td>
                                    <td>{item.Pulser}</td>
                                    <td>{item.ControlTemperature}</td>
                                    <td>{new Date(item.Timestamp).toLocaleString()}</td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                </div>
            </Collapse>
        </Container>
    );
}

export default TemperaturesTable;

