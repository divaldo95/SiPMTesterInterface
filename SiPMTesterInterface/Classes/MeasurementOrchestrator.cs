using System;
using System.Collections.Generic;
using SiPMTesterInterface.ClientApp.Services;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;
using SiPMTesterInterface.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        protected readonly List<LogMessageModel> Logs = new List<LogMessageModel>();

        private readonly ILogger<MeasurementOrchestrator> _logger;

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

        public MeasurementOrchestrator(ILogger<MeasurementOrchestrator> logger)
		{
            globalState = new GlobalStateModel();
            _logger = logger;
        }

        public MeasurementStartModel MeasurementData
        {
            get
            {
                return globalState.CurrentRun;
            }
        }

        public void PrepareMeasurement(MeasurementStartModel measurementStart)
		{
            MeasureDMMResistanceAtBegining = measurementStart.MeasureDMMResistance;
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
                                SiPM = sipm,
                                Vs = sipm.IVVoltages
                            })
                        )
                    )
                );
            var iv = filteredSiPMs.Where(item => item.SiPM.IV == 1).ToList();

            // Output the filtered SiPMs
            //do some shufflin'
            IVMeasurementOrder.Clear(); //empty list if everything went fine until this point
            foreach (var item in iv)
            {
                if (item.Vs.Count == 0 && measurementStart.IVVoltages.Count == 0)
                {
                    _logger.LogWarning($"SiPM ({item.BlockIndex},{item.ModuleIndex},{item.ArrayIndex},{item.SiPMIndex}) is excluded, because its voltage lists are empty");
                }
                else
                {
                    IVMeasurementOrder.Add(new CurrentSiPMModel(item.BlockIndex, item.ModuleIndex, item.ArrayIndex, item.SiPMIndex));
                    Console.WriteLine($"BlockIndex: {item.BlockIndex}, ModuleIndex: {item.ModuleIndex}, ArrayIndex: {item.ArrayIndex}, SiPMIndex: {item.SiPMIndex}, DMMResistance: {item.SiPM.DMMResistance}, IV: {item.SiPM.IV}, SPS: {item.SiPM.SPS}");
                }
            }

            if (IVMeasurementOrder.Count > 0)
            {
                CurrentIVMeasurementIndex = 0; //reset the index counter
            }

            ShuffleIVList(IVMeasurementOrder);

            // Define the valid array combinations
            HashSet<int[]> validArrayCombinations = new HashSet<int[]>
                {
                    new int[] {0, 2, 4, 6},
                    new int[] {1, 3, 5, 7}
                };

            SPSMeasurementOrder.Clear();

            var sps = filteredSiPMs.Where(item => item.SiPM.SPS == 1).ToList();

            var groups = sps.GroupBy(item => new { item.SiPMIndex });

            foreach (var group in groups)
            {
                var module0Arrays = group.Where(item => item.ModuleIndex == 0).Select(item => item.ArrayIndex).ToList();
                var module1Arrays = group.Where(item => item.ModuleIndex == 1).Select(item => item.ArrayIndex).ToList();

                List<int> selectedArrays = new List<int>();
                if (module0Arrays.Intersect(new[] { 0, 2 }).Count() == 2 && module1Arrays.Intersect(new[] { 0, 2 }).Count() == 2)
                {
                    selectedArrays.AddRange(new[] { 0, 2 });
                }
                else if (module0Arrays.Intersect(new[] { 1, 3 }).Count() == 2 && module1Arrays.Intersect(new[] { 1, 3 }).Count() == 2)
                {
                    selectedArrays.AddRange(new[] { 1, 3 });
                }

                var selectedSiPMs = group
                    .Where(item => selectedArrays.Contains(item.ArrayIndex))
                    .Select(item => new CurrentSiPMModel
                    {
                        Block = item.BlockIndex,
                        Module = item.ModuleIndex,
                        Array = item.ArrayIndex,
                        SiPM = item.SiPMIndex
                    })
                    .ToList();

                if (selectedSiPMs.Count > 0)
                {
                    SPSMeasurementOrder.Add(selectedSiPMs);
                }
            }

            if (SPSMeasurementOrder.Count > 0)
            {
                CurrentSPSMeasurementIndex = 0; //reset the index counter
                Console.WriteLine("Non empty SPS data");
            }
            else
            {
                Console.WriteLine("Empty SPS data");
            }
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

        public bool IsBlockOrModuleChanging(out int nextBlock, out int nextModule, out MeasurementType measurementType)
        {
            bool retVal = false;
            nextBlock = -1;
            nextModule = -1;
            measurementType = MeasurementType.Unknown;
            if (MeasureDMMResistanceAtBegining)
            {
                nextBlock = 0;
                nextModule = 0;
                measurementType = MeasurementType.DMMResistanceMeasurement;
                retVal = true;
            }
            // first measurement
            else if (CurrentIVMeasurementIndex == 0)
            {
                nextBlock = IVMeasurementOrder[0].Block;
                nextModule = IVMeasurementOrder[0].Module;
                measurementType = MeasurementType.IVMeasurement;
                retVal = true;
            }
            // Check if there is at least one measurement left and whether it's block is matching with the current one, of not, change
            else if (CurrentIVMeasurementIndex < IVMeasurementOrder.Count && (IVMeasurementOrder[CurrentIVMeasurementIndex - 1].Block != IVMeasurementOrder[CurrentIVMeasurementIndex].Block || IVMeasurementOrder[CurrentIVMeasurementIndex - 1].Module != IVMeasurementOrder[CurrentIVMeasurementIndex].Module))
            {
                nextBlock = IVMeasurementOrder[CurrentIVMeasurementIndex].Block;
                nextModule = IVMeasurementOrder[CurrentIVMeasurementIndex].Module;
                measurementType = MeasurementType.IVMeasurement;
                retVal = true;
            }
            // sps things here
            return retVal;
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
            else if (retVal = IsSPSterationAvailable())
            {
                type = MeasurementType.SPSMeasurement;
                nextData = new SPSStartModel(); //add details
                sipms = SPSMeasurementOrder[CurrentSPSMeasurementIndex];
                CurrentSPSMeasurementIndex++;
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

        protected void SetRetryFailedMeasurement(MeasurementType type)
        {
            if (type == MeasurementType.IVMeasurement && CurrentIVMeasurementIndex > 0)
            {
                CurrentIVMeasurementIndex--;
            }
            else if (type == MeasurementType.SPSMeasurement && CurrentSPSMeasurementIndex > 0)
            {
                CurrentSPSMeasurementIndex--;
            }
            else if (type == MeasurementType.DMMResistanceMeasurement)
            {
                MeasureDMMResistanceAtBegining = true;
            }
        }

        //Private functions-----------------------------------------------------
        private void ShuffleIVList(List<CurrentSiPMModel> list, int maxIterations = 20)
        {
            // Group by ArrayIndex
            CurrentSiPMModel[] arr = new CurrentSiPMModel[list.Count];
            list.CopyTo(arr);
            list.Clear();
            var groupedByIndexes = arr.GroupBy(item => new { item.Block, item.Module, item.Array });
            //var groupedByArrayIndex = arr.GroupBy(item => item.Array);

            //list.Clear();

            foreach (var group in groupedByIndexes)
            {
                bool isValid = false;
                int iterationCounter = 0;
                var newList = group.ToList();
                while (!isValid && iterationCounter < maxIterations)
                {
                    newList.Shuffle();

                    isValid = true;
                    for (int i = 1; i < list.Count; i++)
                    {
                        if (Math.Abs(list[i].SiPM - list[i - 1].SiPM) == 1
                                && list[i].Block == list[i - 1].Block
                                && list[i].Module == list[i - 1].Module
                                && list[i].Array == list[i - 1].Array)
                        {
                            isValid = false;
                            break;
                        }
                    }
                    iterationCounter++;
                }
                if (iterationCounter >= maxIterations && !isValid)
                {
                    _logger.LogWarning($"Max iteration reached without meeting the neighboring criteria for Array {newList.First().Array}");
                }
                list.AddRange(newList);
            }            
        }
    }
}

