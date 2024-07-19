import React, { useState, useEffect } from 'react';
import { Table, Button, Collapse, Container, Accordion } from 'react-bootstrap';

function TemperaturesTable(props) {
    const { className, temperaturesArray } = props;

    const temperaturesToShow = temperaturesArray.slice(-25).reverse();

    return (
        <Container className="mt-5">
            <Accordion defaultActiveKey="0">
                <Accordion.Item eventKey={0}>
                    <Accordion.Header>Temperatures</Accordion.Header>
                    <Accordion.Body>
                        <Table className="w-100" bordered>
                            <thead>
                                <tr>
                                    <th>Block</th>
                                    <th>Module 1</th>
                                    <th>Module 2</th>
                                    <th>Pulser</th>
                                    <th>Control Temperature</th>
                                    <th>Timestamp</th>
                                </tr>
                            </thead>
                            <tbody>
                                {temperaturesToShow.map((item, index) => (
                                    <tr key={index}>
                                        <td>{item.Block}</td>
                                        <td>{item.Module1.join(', ')}</td>
                                        <td>{item.Module2.join(', ')}</td>
                                        <td>{item.Pulser}</td>
                                        <td>{item.ControlTemperature}</td>
                                        <td>{new Date(item.Timestamp * 1000).toLocaleDateString()} {new Date(item.Timestamp * 1000).toTimeString().split(' ')[0]}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </Table>
                    </Accordion.Body>
                </Accordion.Item>
            </Accordion>
           
        </Container>
    );
}

export default TemperaturesTable;

