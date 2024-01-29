﻿import React from 'react';

const Sidebar = () => {
    return (
        <div className="container">
            <div className="d-flex flex-column flex-shrink-0 p-3 bg-light" style={{ width: 300 + 'px', height: '100%', position: 'fixed', left: 0 }}>
                <ul className="nav nav-pills flex-column mb-auto">
                    <li className="nav-item">
                        <a href="#" className="nav-link active" aria-current="page">
                            <svg className="bi me-2" width="16" height="16"></svg>
                            Home
                        </a>
                    </li>
                    <li>
                        <a href="#" className="nav-link link-dark">
                            <svg className="bi me-2" width="16" height="16"></svg>
                            Dashboard
                        </a>
                    </li>
                    <li>
                        <a href="#" className="nav-link link-dark">
                            <svg className="bi me-2" width="16" height="16"></svg>
                            Orders
                        </a>
                    </li>
                    <li>
                        <a href="#" className="nav-link link-dark">
                            <svg className="bi me-2" width="16" height="16"></svg>
                            Products
                        </a>
                    </li>
                    <li>
                        <a href="#" className="nav-link link-dark">
                            <svg className="bi me-2" width="16" height="16"></svg>
                            Customers
                        </a>
                    </li>
                </ul>
                <hr></hr>
                <div className="dropdown">
                    <a href="#" className="d-flex align-items-center link-dark text-decoration-none dropdown-toggle" id="dropdownUser2" data-bs-toggle="dropdown" aria-expanded="false">
                        <img src="https://github.com/mdo.png" alt="" width="32" height="32" className="rounded-circle me-2"></img>
                        <strong>mdo</strong>
                    </a>
                    <ul className="dropdown-menu text-small shadow" aria-labelledby="dropdownUser2">
                        <li><a className="dropdown-item" href="#">New project...</a></li>
                        <li><a className="dropdown-item" href="#">Settings</a></li>
                        <li><a className="dropdown-item" href="#">Profile</a></li>
                        <li><hr className="dropdown-divider"></hr></li>
                        <li><a className="dropdown-item" href="#">Sign out</a></li>
                    </ul>
                </div>
            </div>
        </div>
    );
};

export default Sidebar;