import React from 'react';

function BlockLocation({ blockLocation }) {
    return (
        <svg width="40" height="40">
            {/* Fourth row (3) */}
            <rect x="0" y="0" width="40" height="10" fill={blockLocation === 3 ? 'grey' : 'white'} stroke="grey" strokeWidth="0.8" />

            {/* Third row (2) */}
            <rect x="0" y="10" width="40" height="10" fill={blockLocation === 2 ? 'grey' : 'white'} stroke="grey" strokeWidth="0.8" />

            {/* Second row (1) */}
            <rect x="0" y="20" width="40" height="10" fill={blockLocation === 1 ? 'grey' : 'white'} stroke="grey" strokeWidth="0.8" />

            {/* First row (0) */}
            <rect x="0" y="30" width="40" height="10" fill={blockLocation === 0 ? 'grey' : 'white'} stroke="grey" strokeWidth="0.8" />
        </svg>
    );
}

export default BlockLocation;
