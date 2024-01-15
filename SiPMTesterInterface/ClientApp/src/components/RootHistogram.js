import React, { useEffect, useRef } from 'react';
import * as JSROOT from 'jsroot';

const RootHistogram = () => {
    const rootContainerRef = useRef(null);

    useEffect(() => {
        // Create a random data array
        const data = Array.from({ length: 1000 }, () => Math.floor(Math.random() * 1000));

        // Create a JS ROOT histogram using createHistogram
        const histogram = JSROOT.createHistogram('TH1D', 10); //bins

        // Customize the histogram properties if needed
        histogram.fTitle = 'Random Data Histogram';
        histogram.fXaxis.fTitle = 'X-axis';
        histogram.fYaxis.fTitle = 'Y-axis';
        for (let i = 0; i < 1000; i++) {
            histogram.Fill(data[i]);
        }

        // Draw the histogram in the specified container
        JSROOT.draw(rootContainerRef.current, histogram, 'hist');

        // Clean up when the component is unmounted
        return () => {
            JSROOT.cleanup(rootContainerRef.current);
        };
    }, []);

    return <div ref={rootContainerRef} style={{ width: '100%', height: '400px' }} />;
};

export default RootHistogram;
