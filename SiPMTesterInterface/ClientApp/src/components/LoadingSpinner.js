import React from 'react';
import './LoadingSpinner.css'; // You may want to create a CSS file for styling

const LoadingSpinner = () => {
    return (
        <div className="loading-spinner-container">
            <div className="loading-spinner"></div>
        </div>
    );
};

export default LoadingSpinner;

