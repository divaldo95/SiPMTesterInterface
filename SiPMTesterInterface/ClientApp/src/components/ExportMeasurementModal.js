import { useContext, useState, useEffect } from 'react';
import { MeasurementContext } from '../context/MeasurementContext';
import { Modal, Button, Table, Pagination, Form, Container, Row, Col, Spinner, ListGroup, Accordion, Card } from 'react-bootstrap';
import MeasurementStateService from '../services/MeasurementStateService';
import ExportMessageModal from './ExportMessageModal';
import './ButtonStyles.css';

function ExportMeasurementModal(props) {
    const { showModal, closeModal } = props;
    const [showSkippedModal, setShowSkippedModal] = useState(false);
    const [skippedList, setSkippedList] = useState([]);
    const [exportLocation, setExportLocation] = useState("");

    const { measurementStates } = useContext(MeasurementContext);

    const handleShowSkippedModal = () => setShowSkippedModal(true);
    const handleCloseSkippedModal = () => setShowSkippedModal(false);

    /*
     const anyOk = [
            checks.RForwardOK,
            checks.IDarkOK,
            checks.IVTemperatureOK,
            checks.IVMeasurementOK,
            checks.IVVoltageCheckOK,
            checks.IVCurrentCheckOK,
            checks.IVVbrOK
        ].some(Boolean);

    const allDone = [
        checks.RForwardDone,
        checks.IDarkDone,
        checks.IVDone
    ].every(Boolean);
     */

    const getExportDir = () => {
        try {
            const response = MeasurementStateService.getExportBaseDirectory();

            response.then((resp) => {
                console.log(resp);
                if (resp.ExportDir.slice(-1) === "/") {
                    setExportLocation(resp.ExportDir.slice(0, -1)); // remove '/'
                    setCurrentPath(resp.ExportDir.slice(0, -1));
                }
                else {
                    setExportLocation(resp.ExportDir);
                    setCurrentPath(resp.ExportDir);
                }
            });
        } catch (error) {
            // If the request fails
            console.log(error);
            setVariant('danger');
        }
    }

    useEffect(() => {
        getExportDir();
    }, []);

    const isSiPMFailed = (sipm) => {
        if (!sipm.Checks.SelectedForMeasurement) {
            return false; //not failed
        }

        const allOk = [
            sipm.Checks.RForwardOK,
            sipm.Checks.IDarkOK,
            sipm.Checks.IVTemperatureOK,
            sipm.Checks.IVMeasurementOK,
            sipm.Checks.IVVoltageCheckOK,
            sipm.Checks.IVCurrentCheckOK,
            sipm.Checks.IVVbrOK
        ].every(Boolean);

        return !allOk;
    }

    const isSiPMPassed = (sipm) => {
        if (!sipm.Checks.SelectedForMeasurement) {
            return false; //not passed
        }

        const allOk = [
            sipm.Checks.RForwardOK,
            sipm.Checks.IDarkOK,
            sipm.Checks.IVTemperatureOK,
            sipm.Checks.IVMeasurementOK,
            sipm.Checks.IVVoltageCheckOK,
            sipm.Checks.IVCurrentCheckOK,
            sipm.Checks.IVVbrOK
        ].every(Boolean);
        return allOk;
    }

    const isArrayHasFailedSiPMs = (array) => {
        array.SiPMs.forEach((sipm, sipmIndex) => {
            if (isSiPMFailed(sipm)) {
                return true;
            }
        });
        return false;
    }

    const filterSiPMs = (data, filters) => {
        const filteredSiPMs = [];

        data.Blocks.forEach((block, blockIndex) => {
            block.Modules.forEach((module, moduleIndex) => {
                module.Arrays.forEach((array, arrayIndex) => {
                    const hasFailed = isArrayHasFailedSiPMs(array);
                    //console.log("Array " + arrayIndex + "has failed: " + hasFailed);
                    array.SiPMs.forEach((sipm, sipmIndex) => {
                        if (filters.arrayHasFailedSiPMs) {
                            if (hasFailed) {
                                filteredSiPMs.push({
                                    BlockIndex: blockIndex,
                                    ModuleIndex: moduleIndex,
                                    ArrayIndex: arrayIndex,
                                    SiPMIndex: sipmIndex,
                                    SiPM: sipm
                                });
                            }
                            return;
                        }

                        if (filters.selectedForMeasurement && !sipm.Checks.SelectedForMeasurement) {
                            return;
                        }

                        if (filters.failedSiPMs && !filters.passedSiPMs && !isSiPMFailed(sipm)) {
                            return;
                        }

                        if (filters.passedSiPMs && !filters.failedSiPMs && !isSiPMPassed(sipm)) {
                            return;
                        }

                        filteredSiPMs.push({
                            BlockIndex: blockIndex,
                            ModuleIndex: moduleIndex,
                            ArrayIndex: arrayIndex,
                            SiPMIndex: sipmIndex,
                            SiPM: sipm
                        });
                    });
                });
            });
        });

        return filteredSiPMs;
    };

    //const filteredSiPMs = filterSiPMs(measurementStates);

    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage] = useState(64);
    const [globalFilters, setGlobalFilters] = useState({
        selectedForMeasurement: false,
        passedSiPMs: false,
        failedSiPMs: false,
        arrayHasFailedSiPMs: false
    });
    const [selectedSiPMs, setSelectedSiPMs] = useState([]);
    const [loading, setLoading] = useState(false);
    const [variant, setVariant] = useState('primary');

    const filteredSiPMs = filterSiPMs(measurementStates, globalFilters);

    const indexOfLastItem = currentPage * itemsPerPage;
    const indexOfFirstItem = indexOfLastItem - itemsPerPage;
    const currentItems = filteredSiPMs.slice(indexOfFirstItem, indexOfLastItem);

    const totalPages = Math.ceil(filteredSiPMs.length / itemsPerPage);

    const [currentPath, setCurrentPath] = useState(""); // The current directory path
    const [folders, setFolders] = useState([]); // List of folders in the current directory
    const [pathLoading, setPathLoading] = useState(false); // Loading state
    const [locationBtnVariant, setLocationBtnVariant] = useState('primary');

    const renderFilteredBtnName = () => {
        const anyChecked = [
            globalFilters.selectedForMeasurement,
            globalFilters.passedSiPMs,
            globalFilters.failedSiPMs,
            globalFilters.arrayHasFailedSiPMs
        ].some(Boolean);

        let btnText = filteredSiPMs.every((item) =>
                selectedSiPMs.includes(`${item.BlockIndex}-${item.ModuleIndex}-${item.ArrayIndex}-${item.SiPMIndex}`)
            ) ? 'Deselect all' : 'Select all';
        if (anyChecked) {
            btnText = btnText + " from filtered list";
        }
        return btnText;
    }

    const handleCheckboxChange = (sipmIndex) => {
        setSelectedSiPMs((prevSelected) => {
            if (prevSelected.includes(sipmIndex)) {
                return prevSelected.filter((index) => index !== sipmIndex);
            } else {
                return [...prevSelected, sipmIndex];
            }
        });
    };

    const handleSelectAll = () => {
        const pageSiPMs = currentItems.map((item) => `${item.BlockIndex}-${item.ModuleIndex}-${item.ArrayIndex}-${item.SiPMIndex}`);
        if (selectedSiPMs.length === currentItems.length) {
            setSelectedSiPMs([]);
        } else {
            setSelectedSiPMs(pageSiPMs);
        }
    };

    const addPageSiPMsToSelection = (sipms) => {
        const pageSiPMs = sipms.map(
            (item) => `${item.BlockIndex}-${item.ModuleIndex}-${item.ArrayIndex}-${item.SiPMIndex}`
        );

        // Add only new SiPMs that are not already in selectedSiPMs
        setSelectedSiPMs((prevSelected) => {
            const newSelections = pageSiPMs.filter((sipm) => !prevSelected.includes(sipm));
            return [...prevSelected, ...newSelections]; // Combine old and new selections
        });
    };

    const removePageSiPMsFromSelection = (sipms) => {
        const pageSiPMs = sipms.map(
            (item) => `${item.BlockIndex}-${item.ModuleIndex}-${item.ArrayIndex}-${item.SiPMIndex}`
        );

        setSelectedSiPMs((prevSelected) => prevSelected.filter((sipm) => !pageSiPMs.includes(sipm)));
    };

    const handleSelectAllToggleFiltered = () => {
        const pageSiPMs = filteredSiPMs.map(
            (item) => `${item.BlockIndex}-${item.ModuleIndex}-${item.ArrayIndex}-${item.SiPMIndex}`
        );
        const allSelected = pageSiPMs.every((sipm) => selectedSiPMs.includes(sipm));
        console.log("All selected: " + allSelected);
        if (allSelected) {
            // If all are selected, deselect all
            removePageSiPMsFromSelection(filteredSiPMs);
        } else {
            // Otherwise, select all
            addPageSiPMsToSelection(filteredSiPMs);
        }
    };

    const handleSelectAllToggle = () => {
        const pageSiPMs = currentItems.map(
            (item) => `${item.BlockIndex}-${item.ModuleIndex}-${item.ArrayIndex}-${item.SiPMIndex}`
        );

        const allSelected = pageSiPMs.every((sipm) => selectedSiPMs.includes(sipm));

        if (allSelected) {
            // If all are selected, deselect all
            removePageSiPMsFromSelection(currentItems);
        } else {
            // Otherwise, select all
            addPageSiPMsToSelection(currentItems);
        }
    };

    const exportSelected = async () => {
        setLoading(true);
        setVariant('primary'); // Reset to primary while loading

        const exportData = selectedSiPMs.map((id) => {
            const [block, module, array, sipm] = id.split('-').map(Number);
            return {
                Block: block,
                Module: module,
                Array: array,
                SiPM: sipm
            };
        });

        try {
            // Replace with your actual API URL
            console.log(exportData);
            const response = MeasurementStateService.exportMeasurementData(exportData);

            response.then((resp) => {
                console.log(resp);
                if (resp.status === 200 && resp.data.SiPMs.length === 0) {
                    setVariant('success');
                    closeModal();
                }
                else {
                    setVariant('danger');
                    setSkippedList(resp.data.SiPMs);
                    handleShowSkippedModal();
                }
            });
        } catch (error) {
            // If the request fails
            console.log(error);
            setVariant('danger');
        } finally {
            setLoading(false);
        }
    };

    const selectAllPassedOnly = () => {
        const filter = {
            selectedForMeasurement: false,
            passedSiPMs: true,
            failedSiPMs: false,
            arrayHasFailedSiPMs: false
        }
        const filteredSiPMs = filterSiPMs(measurementStates, filter);

        const pageSiPMs = filteredSiPMs.map((item) => `${item.BlockIndex}-${item.ModuleIndex}-${item.ArrayIndex}-${item.SiPMIndex}`);
        setSelectedSiPMs(pageSiPMs);
    }

    const selectFullyPassedArraysOnly = () => {
        let filter = {
            selectedForMeasurement: false,
            passedSiPMs: true,
            failedSiPMs: false,
            arrayHasFailedSiPMs: false
        }
        const filteredSiPMs = filterSiPMs(measurementStates, filter);

        filter = {
            selectedForMeasurement: false,
            passedSiPMs: false,
            failedSiPMs: false,
            arrayHasFailedSiPMs: true
        }
        const filteredFailedArraySiPMs = filterSiPMs(measurementStates, filter);

        const pageSiPMs = filteredSiPMs.map((item) => `${item.BlockIndex}-${item.ModuleIndex}-${item.ArrayIndex}-${item.SiPMIndex}`);
        setSelectedSiPMs(pageSiPMs);
        removePageSiPMsFromSelection(filteredFailedArraySiPMs);
    }


    const fetchFolders = async (path) => {
        setLoading(true);
        try {
            const normalizedPath = normalizePath(path);
            const response = MeasurementStateService.getDirectoryList(normalizedPath);
            response.then((resp) => {
                console.log(resp);
                setFolders(resp); // Assuming server returns an array of folder names
            });
        } catch (error) {
            console.error("Error fetching folders", error);
        } finally {
            setLoading(false);
        }
    };

    // Fetch folders initially or whenever the currentPath changes
    useEffect(() => {
        fetchFolders(currentPath);
    }, [currentPath]);

    const normalizePath = (path) => {
        return path.replace(/\/+/g, '/'); // Replace multiple '/' with a single '/'
    };

    // Handle folder click
    const handleFolderClick = (folder) => {
        const newPath = normalizePath(`${currentPath}/${folder}`);
        setCurrentPath(newPath);
    };

    // Handle going up a directory
    const handleGoUp = () => {
        const newPath = currentPath.split('/').slice(0, -1).join('/');
        setCurrentPath(normalizePath(newPath)); // Normalize the path when going up
    };

    // Send current path to server
    const handleSendPath = async () => {
        setPathLoading(true);
        try {
            const response = MeasurementStateService.setExportDirectory(currentPath);
            response.then((resp) => {
                setLocationBtnVariant("success");
                setExportLocation(currentPath);
            })
            .catch((err) => {
                console.error('Error sending path', err);
                setLocationBtnVariant("danger");
            });
        } catch (error) {
            
        } finally {
            setPathLoading(false);
            const timer = setTimeout(() => {
                setLocationBtnVariant("primary");
            }, 2000);
            return () => clearTimeout(timer);
        }
    };

    return (
        <Modal scrollable show={showModal} onHide={closeModal} centered size="lg" fullscreen={false}>
            <Modal.Header closeButton>
                <Modal.Title>{`Export measurement`}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <ExportMessageModal show={showSkippedModal} handleClose={handleCloseSkippedModal} skippedList={skippedList}></ExportMessageModal>
                <div className="text-center d-block d-inline-block">
                    <div className="row">
                        <div className="col">
                            <div className="mb-3">
                                <Card className="mb-3">
                                    <Card.Title className="mt-3 mb-0">
                                        Informations
                                    </Card.Title>
                                    <Card.Body className="pt-0">
                                        <hr>
                                        </hr>
                                        Select those SiPMs which data's can be exported. You can select every SiPM even if it has not measured,
                                        but it will be skipped. After a successful export this modal closes. If any of the export is failed
                                        (no available measurement, file system permission issues, etc) a list of the failed exports will show up.
                                    </Card.Body>
                                </Card>
                                    
                                <Accordion className="mb-4">
                                    <Accordion.Item eventKey="0">
                                        <Accordion.Header>Export location: {exportLocation}</Accordion.Header>
                                        <Accordion.Body>
                                            <h5>Current Path: {currentPath || "/"}</h5>
                                            {pathLoading ? (
                                                <Spinner animation="border" />
                                            ) : (
                                                <ListGroup>
                                                    {currentPath && (
                                                        <ListGroup.Item action onClick={handleGoUp}>
                                                            ..
                                                        </ListGroup.Item>
                                                    )}
                                                    {folders.map((folder, index) => (
                                                        <ListGroup.Item key={index} action onClick={() => handleFolderClick(folder)}>
                                                            {folder}
                                                        </ListGroup.Item>
                                                    ))}
                                                </ListGroup>
                                            )}
                                            <Row>
                                                <Col>
                                                    <Button variant={locationBtnVariant} onClick={handleSendPath} className="mt-3" disabled={loading}>
                                                        {loading ? (
                                                            <>
                                                                <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                                                                {' Sending...'}
                                                            </>
                                                        ) : (
                                                            'Set Current Path'
                                                        )}
                                                    </Button>
                                                </Col>
                                                <Col>
                                                    <Button variant={locationBtnVariant} onClick={getExportDir} className="mt-3" disabled={loading}>
                                                        Reset location
                                                    </Button>
                                                </Col>
                                            </Row>
                                            
                                            
                                        </Accordion.Body>
                                    </Accordion.Item>
                                </Accordion>

                                <Card className="mb-3">
                                    <Card.Title className="mt-3 mb-0">
                                        Filters
                                    </Card.Title>
                                    <Card.Body className="pt-0">
                                        <hr>
                                        </hr>
                                        <Row>
                                            <Col>
                                                <ListGroup>
                                                    <ListGroup.Item
                                                        action
                                                        active={globalFilters.selectedForMeasurement}
                                                        onClick={() =>
                                                            setGlobalFilters({
                                                                ...globalFilters,
                                                                selectedForMeasurement: !globalFilters.selectedForMeasurement,
                                                            })
                                                        }
                                                    >
                                                        Show only selected for measurement
                                                    </ListGroup.Item>
                                                    <ListGroup.Item
                                                        action
                                                        active={globalFilters.failedSiPMs}
                                                        onClick={() =>
                                                            setGlobalFilters({
                                                                ...globalFilters,
                                                                failedSiPMs: !globalFilters.failedSiPMs,
                                                            })
                                                        }
                                                    >
                                                        Show failed SiPMs
                                                    </ListGroup.Item>
                                                </ListGroup>
                                            </Col>
                                        </Row>
                                        <Row className="mb-3">
                                            <Col>
                                                <ListGroup>
                                                    <ListGroup.Item
                                                        action
                                                        active={globalFilters.arrayHasFailedSiPMs}
                                                        onClick={() =>
                                                            setGlobalFilters({
                                                                ...globalFilters,
                                                                arrayHasFailedSiPMs: !globalFilters.arrayHasFailedSiPMs,
                                                            })
                                                        }
                                                    >
                                                        Show all SiPMs from arrays which have failed measurement
                                                    </ListGroup.Item>
                                                    <ListGroup.Item
                                                        action
                                                        active={globalFilters.passedSiPMs}
                                                        onClick={() =>
                                                            setGlobalFilters({
                                                                ...globalFilters,
                                                                passedSiPMs: !globalFilters.passedSiPMs,
                                                            })
                                                        }
                                                    >
                                                        Show passed SiPMs
                                                    </ListGroup.Item>
                                                </ListGroup>
                                            </Col>
                                        </Row>
                                    </Card.Body>
                                </Card>

                                <Card className="mb-3">
                                    <Card.Title className="mt-3 mb-0">
                                        Selectors
                                    </Card.Title>
                                    <Card.Body className="pt-0">
                                        <hr>
                                        </hr>
                                        <Row className="mb-3">
                                            <Col className="d-flex justify-content-between">
                                                <Button
                                                    variant="primary"
                                                    onClick={handleSelectAllToggle}
                                                    className="flex-fill btn-equal-size ms-2"
                                                >
                                                    {currentItems.every((item) =>
                                                        selectedSiPMs.includes(`${item.BlockIndex}-${item.ModuleIndex}-${item.ArrayIndex}-${item.SiPMIndex}`)
                                                    )
                                                        ? 'Deselect all from current page'
                                                        : 'Select all from current page'}
                                                </Button>
                                                <Button
                                                    variant="primary"
                                                    onClick={handleSelectAllToggleFiltered}
                                                    className="flex-fill btn-equal-size ms-2"
                                                >
                                                    {renderFilteredBtnName()}
                                                </Button>
                                            </Col>
                                        </Row>

                                        <Row className="mb-3">
                                            <Col className="d-flex justify-content-between">
                                                <Button
                                                    variant="primary"
                                                    onClick={selectAllPassedOnly}
                                                    className="flex-fill btn-equal-size ms-2"
                                                >
                                                    Select passed only SiPMs
                                                </Button>
                                                <Button
                                                    variant="primary"
                                                    onClick={selectFullyPassedArraysOnly}
                                                    className="flex-fill btn-equal-size ms-2"
                                                >
                                                    Select fully passed arrays only
                                                </Button>
                                            </Col>
                                        </Row>
                                    </Card.Body>
                                </Card>

                                
                                
                            </div>

                            <Table responsive striped bordered hover size="sm" className="h-100">
                                <thead>
                                    <tr>
                                        <th>Select</th>
                                        <th>BlockIndex</th>
                                        <th>ModuleIndex</th>
                                        <th>ArrayIndex</th>
                                        <th>SiPMIndex</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {currentItems.map((item, index) => (
                                        <tr key={index}>
                                            <td>
                                                <Form.Check
                                                    type="checkbox"
                                                    checked={selectedSiPMs.includes(`${item.BlockIndex}-${item.ModuleIndex}-${item.ArrayIndex}-${item.SiPMIndex}`)}
                                                    onChange={() => handleCheckboxChange(`${item.BlockIndex}-${item.ModuleIndex}-${item.ArrayIndex}-${item.SiPMIndex}`)}
                                                />
                                            </td>
                                            <td>{item.BlockIndex}</td>
                                            <td>{item.ModuleIndex}</td>
                                            <td>{item.ArrayIndex}</td>
                                            <td>{item.SiPMIndex}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                            <Container className="w-100" fluid>
                                <Pagination>
                                    <Row className="justify-content-center">
                                        <Col className="p-0 m-0">
                                            <Pagination.First onClick={() => setCurrentPage(1)} disabled={currentPage === 1} />
                                        </Col>
                                        <Col className="p-0 m-0">
                                            <Pagination.Prev onClick={() => setCurrentPage((prev) => Math.max(prev - 1, 1))} disabled={currentPage === 1} />
                                        </Col>

                                        {[...Array(totalPages).keys()].map((page) => (
                                            <Col className="p-0 m-0" key={page}>
                                                <Pagination.Item  active={page + 1 === currentPage} onClick={() => setCurrentPage(page + 1)}>
                                                    {page + 1}
                                                </Pagination.Item>
                                            </Col>
                                                
                                        ))}

                                        <Col className="p-0 m-0">
                                            <Pagination.Next onClick={() => setCurrentPage((prev) => Math.min(prev + 1, totalPages))} disabled={currentPage === totalPages} />
                                        </Col>
                                        <Col className="p-0 m-0">
                                            <Pagination.Last onClick={() => setCurrentPage(totalPages)} disabled={currentPage === totalPages} />
                                        </Col>
                                    </Row>
                                        
                                        
                                        
                                        
                                </Pagination>
                            </Container>
                        </div>
                    </div>
                </div>
            </Modal.Body>
            <Modal.Footer>
                <Button variant={variant} onClick={exportSelected} className="ms-2" disabled={loading}>
                    {loading ? (
                        <>
                            <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                            {' Exporting...'}
                        </>
                    ) : (
                        'Export Selected'
                    )}
                </Button>
                <Button variant="secondary" onClick={closeModal}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
}

export default ExportMeasurementModal;
