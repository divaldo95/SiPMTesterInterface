// SelectedList.js
import React from 'react';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';

const SelectedList = ({ data, onReorder }) => {
    const handleDragEnd = (result) => {
        if (!result.destination) {
            return;
        }

        const sourceIndex = result.source.index;
        const destinationIndex = result.destination.index;

        // Notify the parent component about the reordering
        onReorder(sourceIndex, destinationIndex);
    };

    return (
        <DragDropContext onDragEnd={handleDragEnd}>
            <Droppable droppableId="selectedList" direction="horizontal">
                {(provided) => (
                    <div {...provided.droppableProps} ref={provided.innerRef} style={{ display: 'flex' }}>
                        {data.map((item, arrayIndex) => (
                            <Draggable key={item.barcode} draggableId={item.barcode} index={arrayIndex}>
                                {(provided) => (
                                    <div
                                        {...provided.draggableProps}
                                        {...provided.dragHandleProps}
                                        ref={provided.innerRef}
                                        style={{ margin: '10px', border: '1px solid #ccc', padding: '10px' }}
                                    >
                                        <div>
                                            <strong>Barcode:</strong> {item.barcode}
                                        </div>
                                        <div>
                                            <strong>Selected SiPMs:</strong>{' '}
                                            {item.selectedSiPMs.map((sipmPair, sipmIndex) => (
                                                <span key={sipmIndex}>
                                                    {`(${sipmPair[0] !== undefined ? `Left ${sipmPair[0] + 1}` : ''}, ${sipmPair[1] !== undefined ? `Right ${sipmPair[1] + 1}` : ''
                                                        }) `}
                                                </span>
                                            ))}
                                        </div>
                                    </div>
                                )}
                            </Draggable>
                        ))}
                        {provided.placeholder}
                    </div>
                )}
            </Droppable>
        </DragDropContext>
    );
};

export default SelectedList;
