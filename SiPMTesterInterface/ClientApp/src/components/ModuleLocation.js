import React from 'react';

function ModuleLocation({ moduleLocation }) {


    return (
        <svg width="40" height="40">
            {/* First row */}
            <rect x="0" y="0" width="40" height="20" fill={moduleLocation === 1 ? 'grey' : 'white'} stroke="grey" strokeWidth="0.5" />

            {/* Second row */}
            <rect x="0" y="20" width="40" height="20" fill={moduleLocation === 0 ? 'grey' : 'white'} stroke="grey" strokeWidth="0.5" />
        </svg>
    );
}

export default ModuleLocation;
