import SiPMModule from './SiPMModule';

function SiPMBlock(props) {
    const { BlockIndex, ModuleCount, ArrayCount, SiPMCount, className } = props;

    return (
        <>
            {Array.from({ length: ModuleCount }, (_, j) => (
                <div className={`${className}`} key={j}>
                    <div className="row">
                    <div className="col mb-4">
                            <SiPMModule BlockIndex={BlockIndex} ModuleIndex={j} ArrayCount={ArrayCount} SiPMCount={SiPMCount}>
                        </SiPMModule>
                        </div>
                    </div>
                </div>
            ))}
        </>
    );
}

export default SiPMBlock;
