import React, { useEffect, useRef, useState } from 'react';
import * as JSROOT from 'jsroot';

const RootTGraph = (props) => {
    const { x, y, render } = props;
    const rootContainerRef = useRef(null);
    const [graph, setGraph] = useState(null);

    const updateGraph = () => {

    }

    useEffect(() => {
        const len = x.length;
        console.log(len);
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



    if (!render) {
        return null;
    }

    console.log(x);
    console.log(y);
    return <div ref={rootContainerRef} style={{ width: '100%', height: '400px' }} />;
};

export default RootTGraph;
