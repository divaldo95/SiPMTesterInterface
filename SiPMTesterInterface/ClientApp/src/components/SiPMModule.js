import { useContext, Fragment } from 'react';
import ModuleLocation from './ModuleLocation';
import ModeSelectButtonGroup from './ModeSelectButtonGroup';
import SiPMArray from './SiPMArray';

function SiPMModule(props) {
    const { BlockIndex, ModuleIndex, ArrayCount, SiPMCount, className } = props;

    return (
        <>
            <div className={`card ${className}`}>
                <h5 className="card-header">
                    <div className="row align-items-center">
                        <div className="col-auto">
                            <ModuleLocation moduleLocation={ModuleIndex} />
                        </div>

                        <div className="col">
                            <div className="float-end">
                                <ModeSelectButtonGroup BlockIndex={BlockIndex} ModuleIndex={ModuleIndex}> </ModeSelectButtonGroup>
                            </div>
                            
                        </div>
                    </div>

                </h5>
                <div className="card-body">
                    <div className="row">
                        {Array.from({ length: ArrayCount }, (_, j) => (
                            <Fragment key={j}>
                                <div className="col mb-3">
                                    <SiPMArray BlockIndex={BlockIndex} ModuleIndex={ModuleIndex} ArrayIndex={j} SiPMCount={SiPMCount} Editable={ true }>
                                    </SiPMArray>
                                </div>
                                {j % 2 === 1 && <div className="w-100"></div>}
                            </Fragment>
                        ))}
                    </div>
                </div>
            </div>
        </>
    );
}

export default SiPMModule;
