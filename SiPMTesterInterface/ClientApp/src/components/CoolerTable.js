import React, { useState, useEffect } from 'react';
import { Table, Button, Collapse, Container, Accordion } from 'react-bootstrap';

function CoolerTable(props) {
    const { className, coolerArray } = props;

    return (
        <Container className="mt-5">
            <Accordion defaultActiveKey="0">
                <Accordion.Item eventKey={0}>
                    <Accordion.Header>Cooler states</Accordion.Header>
                    <Accordion.Body>
                        <div style={{ overflowX: 'auto' }}>
                            <Table striped className="w-100" bordered hover>
                                <thead>
                                    <tr>
                                        <th>Block</th>
                                        <th>State 1</th>
                                        <th>State 2</th>
                                        <th>Stable 1</th>
                                        <th>Stable 2</th>
                                        <th>Cooler 1 Temperature</th>
                                        <th>Cooler 2 Temperature</th>
                                        <th>Peltier 1 Voltage</th>
                                        <th>Peltier 2 Voltage</th>
                                        <th>Peltier 1 Current</th>
                                        <th>Peltier 2 Current</th>
                                        <th>Timestamp</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {coolerArray.map((item, index) => (
                                        <tr key={index}>
                                            <td>{item.Block}</td>
                                            <td>{item.State1}</td>
                                            <td>{item.State2}</td>
                                            <td>{item.TempStableFlag1}</td>
                                            <td>{item.TempStableFlag2}</td>
                                            <td>{item.Cooler1Temp}</td>
                                            <td>{item.Cooler2Temp}</td>
                                            <td>{item.Peltier1Voltage}</td>
                                            <td>{item.Peltier2Voltage}</td>
                                            <td>{item.Peltier1Current}</td>
                                            <td>{item.Peltier2Current}</td>
                                            <td>{new Date(item.Timestamp * 1000).toLocaleDateString()} {new Date(item.Timestamp * 1000).toTimeString().split(' ')[0]}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </div>
                    </Accordion.Body>
                </Accordion.Item>
            </Accordion>
           
        </Container>
    );
}

export default CoolerTable;

