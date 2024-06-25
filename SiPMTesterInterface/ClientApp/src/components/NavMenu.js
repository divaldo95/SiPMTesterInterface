import React, { useState, useContext } from 'react';
import { Collapse, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Accordion, Spinner, Badge, NavDropdown } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';
import { LogContext } from '../context/LogContext';
import { MeasurementContext } from '../context/MeasurementContext';
import { TaskTypes, TaskTypesString } from '../enums/TaskTypes';

const NavMenu = () => {
    const [collapsed, setCollapsed] = useState(true);

    const toggleNavbar = () => {
        setCollapsed(!collapsed);
    };

    const { logs, fetchLogs, updateLogsResolved, unresolvedLogs, appendLog, unresolvedLogCount, currentError } = useContext(LogContext);
    const { instrumentStatuses, updateCurrentTask, canToggleMeasurementView,
            toggleMeasurementView, measurementDataView, handleShowLogModal,
            handleCloseLogModal, handleShowPulserLEDModal } = useContext(MeasurementContext);
    
    return (
        <header>
            <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow" container light color="light" fixed="top">
                <NavbarBrand tag={Link} to="/">SiPMTesterInterface</NavbarBrand>
                <NavbarToggler onClick={toggleNavbar} className="mr-2" />
                <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!collapsed} navbar>
                    <ul className="navbar-nav flex-grow">
                        <NavItem>
                            <NavLink tag={Link} onClick={handleShowLogModal} className="text-dark"><Badge bg={`${unresolvedLogCount > 0 ? "danger" : "success"}`}>{unresolvedLogCount}</Badge></NavLink>
                        </NavItem>
                        <NavItem>
                            <NavLink tag={Link} className="text-dark"><Badge bg="primary">{TaskTypesString(instrumentStatuses.CurrentTask)}</Badge></NavLink>
                        </NavItem>
                        <NavDropdown title={<Badge bg="primary">1</Badge>} id="basic-nav-dropdown">
                            <NavDropdown.Item><Badge bg="primary">IV: 0, 0, 4, 2</Badge></NavDropdown.Item>
                        </NavDropdown>
                        <NavItem>
                            <NavLink tag={Link} className="text-dark" disabled={!canToggleMeasurementView()} onClick={toggleMeasurementView}>{measurementDataView ? <Badge bg="primary"><i className="bi bi-file-earmark-plus"></i></Badge> : <Badge bg="primary"><i className="bi bi-file-earmark-play"></i></Badge>}</NavLink>
                        </NavItem>
                        <NavItem>
                            
                            <NavLink tag={Link} className="text-dark" onClick={handleShowPulserLEDModal}><Badge bg="primary"><i className="bi bi-lightbulb"></i></Badge></NavLink>
                        </NavItem>
                        <NavItem>
                            <NavLink tag={Link} className="text-dark" to="/sipm">Measurement</NavLink>
                        </NavItem>
                    </ul>
                </Collapse>
            </Navbar>
        </header>
    );
};

export default NavMenu;
