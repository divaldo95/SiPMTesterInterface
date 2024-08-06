import React, { useState, useRef, useCallback, useEffect } from 'react';
import { Modal, Carousel, Button, Form, Row, Col, Container } from 'react-bootstrap';
import { useMeasurement } from '../context/MeasurementContext';
import 'bootstrap/dist/css/bootstrap.min.css';
import ArraySettingsComponent from './ArraySettingsComponent';
import debounce from 'lodash.debounce';
import MeasurementStateService from '../services/MeasurementStateService';

function MeasurementWizard({ show, onHide }) {
    const { measurementData, updateBarcode, updateVopData } = useMeasurement();
    const [carouselIndex, setCarouselIndex] = useState(0);
    const [isBarcodeFetching, setIsBarcodeFetching] = useState(false);
    const inputRefs = useRef([]);

    const [blockIndex, setBlockIndex] = useState(0);
    const [moduleIndex, setModuleIndex] = useState(0);
    const [arrayIndex, setArrayIndex] = useState(0);

    useEffect(() => {
        setCarouselIndex(0);
    }, [show]);

    const barcodes = measurementData.Blocks.flatMap((block, blockIndex) =>
        block.Modules.flatMap((module, moduleIndex) =>
            module.Arrays.map((array, arrayIndex) => ({
                Barcode: array.Barcode,
                BlockIndex: blockIndex,
                ModuleIndex: moduleIndex,
                ArrayIndex: arrayIndex,
            }))
        )
    );

    useEffect(() => {
        if (inputRefs.current[carouselIndex]) {
            inputRefs.current[carouselIndex].focus();
        }
        const currentBarcode = barcodes[carouselIndex];
        setBlockIndex(currentBarcode.BlockIndex);
        setModuleIndex(currentBarcode.ModuleIndex);
        setArrayIndex(currentBarcode.ArrayIndex);
        console.log(currentBarcode.BlockIndex, currentBarcode.ModuleIndex, currentBarcode.ArrayIndex);
    }, [carouselIndex, barcodes]);

    const handleNextCarousel = () => {
        setCarouselIndex((prevIndex) => (prevIndex + 1) % barcodes.length);
    };

    const UpdateVopsAndEnable = (serverResponse, BlockIndex, ModuleIndex, ArrayIndex) => {
        serverResponse.Channels.forEach((channel) => {
            updateVopData(BlockIndex, ModuleIndex, ArrayIndex, channel.ChNo - 1, channel.Vop);
        });
    };

    const fillVopWithZero = (BlockIndex, ModuleIndex, ArrayIndex) => {
        for (let i = 0; i < measurementData.Blocks[BlockIndex].Modules[ModuleIndex].Arrays[ArrayIndex].SiPMs.length; i++) {
            updateVopData(BlockIndex, ModuleIndex, ArrayIndex, i, 0.0);
        }
    };

    const handleSearch = (barcode, BlockIndex, ModuleIndex, ArrayIndex) => {
        MeasurementStateService.getArrayPropertiesBySN(barcode)
            .then((resp) => {
                UpdateVopsAndEnable(resp, BlockIndex, ModuleIndex, ArrayIndex);
                setIsBarcodeFetching(false);
                handleNextCarousel();
            })
            .catch((err) => {
                setIsBarcodeFetching(false);
                fillVopWithZero(BlockIndex, ModuleIndex, ArrayIndex);
                console.error(err);
            });
    };

    const debounceFn = useCallback(debounce(handleSearch, 2000), []);

    const handleBarcodeChange = (e) => {
        const currentBarcode = barcodes[carouselIndex];
        updateBarcode(currentBarcode.BlockIndex, currentBarcode.ModuleIndex, currentBarcode.ArrayIndex, e.target.value);
        debounceFn(e.target.value, currentBarcode.BlockIndex, currentBarcode.ModuleIndex, currentBarcode.ArrayIndex);
        setIsBarcodeFetching(true);
    };

    const handleCarouselSelect = (selectedIndex) => {
        setCarouselIndex(selectedIndex);
    };

    const customArrowStyles = {
        fontSize: '2rem',
        color: 'black'
    };

    if (!show) {
        return null;
    }

    return (
        <Modal show={show} onHide={onHide} centered size="lg" scrollable backdrop="static" centered>
            <Modal.Header closeButton>
                <Modal.Title>Block: {blockIndex}, Module: {moduleIndex}, Array: {arrayIndex}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Carousel
                    activeIndex={carouselIndex}
                    onSelect={handleCarouselSelect}
                    interval={null}
                    controls={true}
                    indicators={false}
                    nextIcon={<i className="bi bi-chevron-right" style={customArrowStyles}></i>}
                    prevIcon={<i className="bi bi-chevron-left" style={customArrowStyles}></i>}
                    fade
                >
                    {barcodes.map((item, index) => (
                        <Carousel.Item key={index}>
                            <Container fluid>
                                <Row className="justify-content-center">
                                    <Col xs={8} md={8}>
                                        <ArraySettingsComponent
                                            show={index === carouselIndex}
                                            BlockIndex={blockIndex}
                                            ModuleIndex={moduleIndex}
                                            ArrayIndex={arrayIndex}
                                            handleBarcodeChange={handleBarcodeChange}
                                            isBarcodeFetching={isBarcodeFetching}
                                            inputRef={(el) => (inputRefs.current[index] = el)}
                                        />
                                    </Col>
                                </Row>
                            </Container>
                        </Carousel.Item>
                    ))}
                </Carousel>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="primary" onClick={onHide}>
                    Save Changes
                </Button>
            </Modal.Footer>
        </Modal>
    );
}

export default MeasurementWizard;
