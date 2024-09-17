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
            handleCloseLogModal, handleShowPulserLEDModal, activeSiPMs, handleShowMeasurementWizard,
            handleShowExcelExportModal } = useContext(MeasurementContext);


    const renderActiveSiPMS = () => {
        //console.log(activeSiPMs.length);
        if (activeSiPMs.length === 0) {
            return (
                <NavItem>
                    <NavLink tag={Link} className="text-dark"><Badge bg="primary">0</Badge></NavLink>
                </NavItem>
            );
        }
        else if (activeSiPMs.length === 1) {
            return (
                <NavItem>
                    <NavLink tag={Link} className="text-dark"><Badge bg="primary">{activeSiPMs[0].Block}, {activeSiPMs[0].Module}, {activeSiPMs[0].Array}, {activeSiPMs[0].SiPM}</Badge></NavLink>
                </NavItem>
            );
        }
        else {
            return (
                <NavDropdown title={<Badge bg="primary">{activeSiPMs.length}</Badge>} id="basic-nav-dropdown">
                    {activeSiPMs.map((sipm, index) => (
                        <NavDropdown.Item key={index}><Badge bg="primary">{sipm.Block}, {sipm.Module}, {sipm.Array}, {sipm.SiPM}</Badge></NavDropdown.Item>
                    ))}
                </NavDropdown>
            );
        }
    };

    const renderWizardButton = () => {
        if (!measurementDataView) {
            return (
                <NavItem>
                    <NavLink onClick={handleShowMeasurementWizard} tag={Link} className="text-dark"><Badge bg="info text-dark"><i className="bi bi-magic"></i></Badge></NavLink>
                </NavItem>
            );
        }
        else {
            return null;
        }
    };

    const renderExcelExportButton = () => {
        if (measurementDataView) {
            return (
                <NavItem>
                    <NavLink onClick={handleShowExcelExportModal} tag={Link} className="text-dark"><Badge bg="info text-dark"><i className="bi bi-box-arrow-up"></i></Badge></NavLink>
                </NavItem>
            );
        }
        else {
            return null;
        }
    };

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
                        {renderActiveSiPMS()}
                        <NavItem>
                            <NavLink tag={Link} className="text-dark" disabled={!canToggleMeasurementView()} onClick={toggleMeasurementView}>{measurementDataView ? <Badge bg="primary"><i className="bi bi-file-earmark-plus"></i></Badge> : <Badge bg="primary"><i className="bi bi-file-earmark-play"></i></Badge>}</NavLink>
                        </NavItem>
                        {renderWizardButton()}
                        {renderExcelExportButton()}
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
