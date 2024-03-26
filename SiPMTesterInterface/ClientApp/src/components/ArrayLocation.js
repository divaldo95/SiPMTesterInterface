import React from 'react';

function ArrayLocation({ arrayLocation }) {
    const arrayCalculatedLocation = (arrayLocation % 4);

    return (
        <svg width="40" height="40">
            {/* First row */}
            <rect x="0" y="0" width="20" height="20" fill={arrayCalculatedLocation === 1 ? 'grey' : 'white'} stroke="grey" strokeWidth="0.5" />
            <rect x="20" y="0" width="20" height="20" fill={arrayCalculatedLocation === 3 ? 'grey' : 'white'} stroke="grey" strokeWidth="0.5" />

            {/* Second row */}
            <rect x="0" y="20" width="20" height="20" fill={arrayCalculatedLocation === 0 ? 'grey' : 'white'} stroke="grey" strokeWidth="0.5" />
            <rect x="20" y="20" width="20" height="20" fill={arrayCalculatedLocation === 2 ? 'grey' : 'white'} stroke="grey" strokeWidth="0.5" />
        </svg>
    );
}

export default ArrayLocation;