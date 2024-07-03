import React, { useEffect, useRef, useState } from 'react';
import * as JSROOT from 'jsroot';
import MeasurementStateService from '../services/MeasurementStateService';
import { Form, Spinner, Button, Tabs, Tab, Alert } from 'react-bootstrap';

const RootTGraph = (props) => {
    const { x, y, render, BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex } = props;
    const rootContainerRef = useRef(null);
    const ivFitContainerRef = useRef(null);
    const [graph, setGraph] = useState(null);

    const [objects, setObjects] = useState([]);
    const [selectedObject, setSelectedObject] = useState('');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const containerRef = useRef(null);

    const handleObjectChange = async (e) => {
        setSelectedObject(e.target.value);
        setLoading(true);

        try {
            const data = await MeasurementStateService.getIVRootFile(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex);

            // Convert the response to a Blob
            const rootUrl = URL.createObjectURL(data);

            // Open and display the selected object from the ROOT file using JSROOT
            const file = await JSROOT.openFile(rootUrl);
            const obj = await file.readObject(e.target.value);

            

            if (containerRef.current) {
                JSROOT.draw(containerRef.current, obj, 'colz');
            }

            const ivFitObj = await file.readObject("fitted_data;1");

            if (ivFitContainerRef.current) {
                JSROOT.draw(ivFitContainerRef.current, ivFitObj, 'colz');
            }

            URL.revokeObjectURL(rootUrl); // Clean up URL object
            setLoading(false);
        } catch (err) {
            setLoading(false);
            setError(err.message);
            console.log(err);
        }
    };

    useEffect(() => {
        const fetchRootFile = async () => {
            try {
                setLoading(true);
                setError(null);

                const data = await MeasurementStateService.getIVRootFile(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex);

                // Convert the response to a Blob
                const rootUrl = URL.createObjectURL(data);

                // Open the ROOT file using JSROOT and get the list of objects
                const file = await JSROOT.openFile(rootUrl);
                const tfileObject = await file.readKeys();
                console.log(tfileObject);
                const objectsList = tfileObject.fKeys.map(key => key.fName + ';' + key.fCycle);

                console.log('Objects in ROOT file:', objectsList); // Debug: log objects

                setObjects(objectsList);
                if (objectsList.length > 0) {
                    setSelectedObject(objectsList[0]);
                }

                URL.revokeObjectURL(rootUrl); // Clean up URL object
                setLoading(false);

            } catch (err) {
                setLoading(false);
                setError(err.message);
                console.log(err);
            }
        };

        fetchRootFile();

        let len = 0;
        if (x && y) {
            len = Math.min(x.length, y.length);
            console.log(len);
        }

        const g = JSROOT.createTGraph(len, x, y);
        g.fTitle = 'Data';
        g.fLineStyle = 3;
        g.fLineColor = 1;
        g.fLineWidth = 3;
        g.fMarkerSize = 3;
        g.fMarkerStyle = 7;
        setGraph(g);
        console.log(g);
        JSROOT.draw(rootContainerRef.current, g, 'graph');
        // Clean up when the component is unmounted
        return () => {
            JSROOT.cleanup(rootContainerRef.current);
        };
    }, [render]);

    useEffect(() => {
        const displaySelectedObject = async () => {
            if (!selectedObject) return;

            setLoading(true);

            try {
                const data = await MeasurementStateService.getIVRootFile(BlockIndex, ModuleIndex, ArrayIndex, SiPMIndex);

                // Convert the response to a Blob
                const rootUrl = URL.createObjectURL(data);

                // Open and display the selected object from the ROOT file using JSROOT
                const file = await JSROOT.openFile(rootUrl);
                console.log(selectedObject);
                const obj = await file.readObject(selectedObject);

                console.log('Displaying object:', selectedObject); // Debug: log selected object

                // Clear the drawing area
                if (containerRef.current) {
                    JSROOT.cleanup(containerRef.current);
                    JSROOT.draw(containerRef.current, obj, 'colz');
                }

                URL.revokeObjectURL(rootUrl); // Clean up URL object
                setLoading(false);
            } catch (err) {
                setLoading(false);
                setError(err.message);
                console.log(err);
            }
        };

        displaySelectedObject();
    }, [selectedObject]);

    if (!render) {
        return null;
    }

    console.log(x);
    console.log(y);
    return (
        <Tabs defaultActiveKey="analysedData" id="root-viewer-tabs">
            <Tab eventKey="rawData" title="RAW Data">
                <div ref={rootContainerRef} style={{ width: '100%', height: '400px' }} />
            </Tab>
            <Tab eventKey="analysedData" title="Analysation result">
                <div ref={ivFitContainerRef} style={{ width: '100%', height: '400px' }} />
                {!loading && !error && (
                    <Form.Group>
                        <Form.Label>Select Object to Display</Form.Label>
                        <Form.Control
                            as="select"
                            value={selectedObject}
                            onChange={handleObjectChange}
                        >
                            {objects.map((objectName, index) => (
                                <option key={index} value={objectName}>
                                    {objectName}
                                </option>
                            ))}
                        </Form.Control>
                    </Form.Group>
                )}
                {!loading && error && (
                    <Alert key="rootFileError" variant="danger">
                        {error}
                    </Alert>
                )}
                
                <div ref={containerRef} style={{ width: '100%', height: '400px' }} />
            </Tab>
        </Tabs>
    );
    
};

export default RootTGraph;
