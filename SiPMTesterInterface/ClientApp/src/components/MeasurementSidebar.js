import React, { useState, useContext } from 'react';
import { Sidebar, Menu, MenuItem, SubMenu } from 'react-pro-sidebar';
import { Accordion, Spinner, Badge } from 'react-bootstrap';
import { LogContext } from '../context/LogContext';
import { MeasurementContext } from '../context/MeasurementContext';
import { TaskTypes, TaskTypesString } from '../enums/TaskTypes';

const MeasurementSidebar = ({ isOpen, toggleSidebar, openErrorsModal, collapsed, toggleCollapsed }) => {
    const [toggled, setToggled] = useState(false);
    const { instrumentStatuses, updateCurrentTask, canToggleMeasurementView, toggleMeasurementView, measurementDataView } = useContext(MeasurementContext);
    const { logs, fetchLogs, updateLogsResolved, unresolvedLogs, appendLog, unresolvedLogCount, currentError } = useContext(LogContext);

    return (
        <Sidebar
            className="bg-light"
            backgroundColor={"rgb(251, 251, 251)"}
            style={{ position: 'fixed', overflowY: 'auto', height: '100%', zIndex: 999 }}
            collapsed={collapsed}
        >
            <Menu className="text-center">
                <MenuItem icon={<Badge bg={`${unresolvedLogCount > 0 ? "danger" : "success"}`}>{unresolvedLogCount}</Badge>} onClick={openErrorsModal}>Errors</MenuItem>
                <MenuItem icon={<Badge bg="primary">{TaskTypesString(instrumentStatuses.currentTask)}</Badge>}>Current task</MenuItem>
                <hr></hr>
                <SubMenu label="Measurements" icon={<Badge bg="primary">1</Badge>}>
                    <MenuItem><Badge bg="primary">IV: 0, 0, 4, 2</Badge></MenuItem>
                </SubMenu>
                <hr></hr>
                <MenuItem disabled={!canToggleMeasurementView()} onClick={toggleMeasurementView} icon={measurementDataView ? <i className="bi bi-file-earmark-plus"></i> : <i className="bi bi-file-earmark-play"></i>}>{measurementDataView ? "New measurement" : "Show results"}</MenuItem>
                <hr></hr>
                <MenuItem onClick={toggleCollapsed}>{collapsed ? <i className="bi bi-arrow-right-square"></i> : <i className="bi bi-arrow-left-square"></i>}</MenuItem>
            </Menu>
        </Sidebar>
    );
};

export default MeasurementSidebar;
