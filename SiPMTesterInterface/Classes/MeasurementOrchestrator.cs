using System;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
	public class MeasurementOrchestrator
	{
        //Only one IV measurement at a time
        private int CurrentIVMeasurementIndex = -1; //-1 - No measurement available, 0 -> IVMeasurementOrder.Count - Actual measurement index
        public List<CurrentSiPMModel> IVMeasurementOrder { get; private set; } = new List<CurrentSiPMModel>();

        //Up to 8 SPS measurements simultenaously
        private int CurrentSPSMeasurementIndex = -1; //Same as IVMeasurementIndex
        public List<List<CurrentSiPMModel>> SPSMeasurementOrder { get; private set; } = new List<List<CurrentSiPMModel>>(); //don't forget to create the second list

        protected GlobalStateModel globalState = new GlobalStateModel();

        public int IVCount
        {
            get
            {
                return IVMeasurementOrder.Count;
            }
        }

        public int IVCurrent
        {
            get
            {
                return CurrentIVMeasurementIndex + 1;
            }
        }

        public MeasurementOrchestrator()
		{
		}

		public void PrepareMeasurement(MeasurementStartModel measurementStart)
		{
            globalState.CurrentRun = measurementStart;
            // Flatten the structure, include indices, and filter items where SiPM.IV == 1
            var filteredSiPMs = globalState.CurrentRun.Blocks
                .SelectMany((block, blockIdx) => block.Modules
                    .SelectMany((module, moduleIdx) => module.Arrays
                        .SelectMany((array, arrayIdx) => array.SiPMs
                            .Select((sipm, sipmIdx) => new
                            {
                                BlockIndex = blockIdx,
                                ModuleIndex = moduleIdx,
                                ArrayIndex = arrayIdx,
                                SiPMIndex = sipmIdx,
                                SiPM = sipm
                            })
                        )
                    )
                )
                .Where(item => item.SiPM.IV == 1)
                .ToList();

            // Output the filtered SiPMs
            //do some shufflin'
            IVMeasurementOrder.Clear(); //empty list if everything went fine until this point
            foreach (var item in filteredSiPMs)
            {
                IVMeasurementOrder.Add(new CurrentSiPMModel(item.BlockIndex, item.ModuleIndex, item.ArrayIndex, item.SiPMIndex));
                Console.WriteLine($"BlockIndex: {item.BlockIndex}, ModuleIndex: {item.ModuleIndex}, ArrayIndex: {item.ArrayIndex}, SiPMIndex: {item.SiPMIndex}, DMMResistance: {item.SiPM.DMMResistance}, IV: {item.SiPM.IV}, SPS: {item.SiPM.SPS}");
            }

            CurrentIVMeasurementIndex = 0; //reset the index counter
        }

        public bool IsIVIterationAvailable()
        {
            return (CurrentIVMeasurementIndex < IVMeasurementOrder.Count);
        }

        public bool IsSPSterationAvailable()
        {
            return (CurrentSPSMeasurementIndex < SPSMeasurementOrder.Count);
        }

        public bool IsIterationAvailable()
        {
            return (IsIVIterationAvailable() || IsSPSterationAvailable());
        }

        /*
         * Pass back the necessary information to start the next round
         * Do some magic here and start IV and/or SPS (simultaneously)
         * TODO: SPSStartModel and related stuff
         */
        public bool GetNextIterationData(out NIMachineStartModel measurementData)
        {
            measurementData = new NIMachineStartModel();
            bool retVal = IsIVIterationAvailable();
            if (retVal)
            {
                measurementData.CurrentSiPM = IVMeasurementOrder[CurrentIVMeasurementIndex]; //return the next one
                SiPM sipm = globalState.CurrentRun
                            .Blocks[measurementData.CurrentSiPM.Block]
                            .Modules[measurementData.CurrentSiPM.Module]
                            .Arrays[measurementData.CurrentSiPM.Array]
                            .SiPMs[measurementData.CurrentSiPM.SiPM];
                measurementData.MeasureDMMResistance = (sipm.DMMResistance > 0);
                List<double> vs = sipm.IVVoltages;
                if (vs.Count != 0)
                {
                    measurementData.Voltages = vs; //Voltage list can be overriden per SiPM
                }
                else
                {
                    measurementData.Voltages = globalState.CurrentRun.IVVoltages;
                }
            }
            else
            {
                measurementData.CurrentSiPM = new CurrentSiPMModel(); //return with -1s
                measurementData.Voltages = new List<double>();
            }
            measurementData.IVSettings = globalState.GlobalIVSettings;
            measurementData.MeasureDMMResistance = (globalState.CurrentRun.MeasureDMMResistance || measurementData.MeasureDMMResistance); //if globally or manually enabled
            CurrentIVMeasurementIndex++; //increment IV index counter
            return retVal;
        }
	}
}

