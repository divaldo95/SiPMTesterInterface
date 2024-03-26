import { useRef } from 'react';

function FileSelectCard(props) {
    const { handleFileResult, inputText, className } = props;
    const fileInputRef = useRef(null);

    const readFile = (file) => {
        if (file) {
            const reader = new FileReader();

            reader.onload = (e) => {
                try {
                    const openedJson = JSON.parse(e.target.result);
                    handleFileResult(openedJson);
                } catch (error) {
                    console.error('Error parsing JSON file:', error);
                }
            };

            reader.readAsText(file);
        }
    };

    const handleImport = (event) => {
        event.preventDefault();
        const file = event.target.files[0];
        readFile(file);
    };

    const handleDrop = (event) => {
        event.preventDefault();
        const file = event.dataTransfer.files[0];
        readFile(file);
    };

    const handleDragOver = (event) => {
        event.preventDefault();
    };

    const handleDragAndDropClick = () => {
        fileInputRef.current.click();
    };

    return (
        <>
            <div className={`card ${className}`}>
                <h5 className="card-header">{ inputText }</h5>
                <div className="card-body">
                    <div className="d-none justify-content-center">
                        <input
                            type="file"
                            accept=".json"
                            ref={fileInputRef}
                            style={{ display: 'none' }}
                            onChange={handleImport}
                        />
                    </div>
                    <div
                        className="border text-center p-3 bg-light"
                        onClick={handleDragAndDropClick}
                        onDrop={handleDrop}
                        onDragOver={handleDragOver}
                        style={{ cursor: 'pointer' }}
                    >
                        Click or drag and drop a JSON file to import
                    </div>
                </div>
            </div>
        </>
    );
}

export default FileSelectCard;