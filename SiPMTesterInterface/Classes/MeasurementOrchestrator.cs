using System;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
	public class MeasurementOrchestrator
	{
        private bool MeasureDMMResistanceAtBegining = true;

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
            globalState = new GlobalStateModel();
		}

		public void PrepareMeasurement(MeasurementStartModel measurementStart)
		{
            MeasureDMMResistanceAtBegining = true;
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
            return (CurrentIVMeasurementIndex < IVMeasurementOrder.Count && CurrentIVMeasurementIndex >= 0);
        }

        public bool IsSPSterationAvailable()
        {
            return (CurrentSPSMeasurementIndex < SPSMeasurementOrder.Count && CurrentSPSMeasurementIndex >= 0);
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
        public bool GetNextIterationData(out MeasurementType type, out object nextData, out List<CurrentSiPMModel> sipms)
        {
            sipms = new List<CurrentSiPMModel>();
            if (MeasureDMMResistanceAtBegining)
            {
                MeasureDMMResistanceAtBegining = false; //run at the beginning
                type = MeasurementType.DMMResistanceMeasurement;
                NIDMMStartModel startModel = new NIDMMStartModel();
                startModel.Identifier = new MeasurementIdentifier(MeasurementType.DMMResistanceMeasurement);
                startModel.DMMResistance = globalState.CurrentRun.DMMResistance;
                nextData = startModel;
                return true;
            }

            bool retVal = IsIVIterationAvailable();
            if (retVal)
            {
                CurrentSiPMModel currentSiPM = IVMeasurementOrder[CurrentIVMeasurementIndex];
                type = MeasurementType.IVMeasurement;
                sipms.Add(currentSiPM); //return the next one

                NIIVStartModel ivStart = new NIIVStartModel();
                ivStart.Identifier = new MeasurementIdentifier(MeasurementType.IVMeasurement);
                ivStart.IVSettings = globalState.GlobalIVSettings;

                SiPM sipm = globalState.CurrentRun
                            .Blocks[currentSiPM.Block]
                            .Modules[currentSiPM.Module]
                            .Arrays[currentSiPM.Array]
                            .SiPMs[currentSiPM.SiPM];
                //measurementData.MeasureDMMResistance = (sipm.DMMResistance > 0);
                List<double> vs = sipm.IVVoltages;
                if (vs.Count != 0)
                {
                    ivStart.Voltages = vs; //Voltage list can be overriden per SiPM
                }
                else
                {
                    ivStart.Voltages = globalState.CurrentRun.IVVoltages;
                }
                CurrentIVMeasurementIndex++; //increment IV index counter
                nextData = ivStart;
            }
            else
            {
                type = MeasurementType.Unknown;
                nextData = new object();
            }
            //measurementData.IVSettings = globalState.GlobalIVSettings;
            //measurementData.MeasureDMMResistance = (globalState.CurrentRun.MeasureDMMResistance || measurementData.MeasureDMMResistance); //if globally or manually enabled
            
            return retVal;
        }
	}
}

