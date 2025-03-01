﻿using System;
using System.Collections.Generic;
using SiPMTesterInterface.ClientApp.Services;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;
using SiPMTesterInterface.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SiPMTesterInterface.Classes
{
    public enum MeasurementOrder
    {
        DCLC = 0,
        DCDC = 1,
        FR = 2,
        IV = 3
    }

	public class MeasurementOrchestrator
	{
        //Final measurement order - Contains all steps
        private int CurrentMeasurementIndex = -1;
        public List<MeasurementOrderModel> FinalMeasurementOrder { get; private set; } = new List<MeasurementOrderModel>();

        private bool MeasureDMMResistanceAtBegining = true;

        //Only one IV measurement at a time
        private int CurrentIVMeasurementIndex = -1; //-1 - No measurement available, 0 -> IVMeasurementOrder.Count - Actual measurement index
        public List<CurrentSiPMModel> IVMeasurementOrder { get; private set; } = new List<CurrentSiPMModel>();

        private int CurrentDCLCMeasurementIndex = -1; //Dark current measurement is broke into two parts: Leakage Current and Dark Current
        private int CurrentDCDCMeasurementIndex = -1;
        public List<CurrentSiPMModel> DCMeasurementOrder { get; private set; } = new List<CurrentSiPMModel>();

        private int CurrentFRMeasurementIndex = -1; //-1 - No measurement available, 0 -> IVMeasurementOrder.Count - Actual measurement index
        public List<CurrentSiPMModel> FRMeasurementOrder { get; private set; } = new List<CurrentSiPMModel>();

        private int CurrentBlockChangingIndex = -1;

        //Up to 8 SPS measurements simultenaously
        private int CurrentSPSMeasurementIndex = -1; //Same as IVMeasurementIndex
        public List<List<CurrentSiPMModel>> SPSMeasurementOrder { get; private set; } = new List<List<CurrentSiPMModel>>(); //don't forget to create the second list

        protected readonly List<LogMessageModel> Logs = new List<LogMessageModel>();

        private readonly ILogger<MeasurementOrchestrator> _logger;

        protected GlobalStateModel globalState = new GlobalStateModel();

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

        //Current measurement type for actual IV enabled SiPM
        //Dark Current - LeakageCurrent - Forward Resistance - IV
        private MeasurementOrder CurrentMeasurementOrderCounter = (MeasurementOrder)0;

        public void PrepareMeasurement(MeasurementStartModel measurementStart)
		{
            CurrentMeasurementOrderCounter = 0;
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
                                Vs = sipm.IVVoltages,
                                Vop = sipm.OperatingVoltage
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
                if (item.Vs.Count == 0 && measurementStart.IVVoltages.Count == 0 && item.Vop > 0)
                {
                    _logger.LogWarning($"SiPM ({item.BlockIndex},{item.ModuleIndex},{item.ArrayIndex},{item.SiPMIndex}) voltage lists are empty but operating voltage is set");
                    IVMeasurementOrder.Add(new CurrentSiPMModel(item.BlockIndex, item.ModuleIndex, item.ArrayIndex, item.SiPMIndex));
                }
                else if (item.Vs.Count == 0 && measurementStart.IVVoltages.Count == 0)
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
                ShuffleIVList(IVMeasurementOrder);
                FinalMeasurementOrder.Clear();
            }

            TaskTypes taskType;
            MeasurementType t;
            object d;
            List<CurrentSiPMModel> s;
            bool b;

            bool prevBlockChangeState = false;
            while (GetNextIterationDataNewOrderSE(out t, out d, out s, out b, true))
            {
                MeasurementOrderModel m;
                if (b && !prevBlockChangeState)
                {
                    if (CurrentIVMeasurementIndex > 0)
                    {
                        m = new MeasurementOrderModel
                        {
                            Task = TaskTypes.BlockDisable,
                            Type = MeasurementType.Unknown,
                            SiPM = IVMeasurementOrder[CurrentIVMeasurementIndex - 1],
                            StartModel = d
                        };
                        FinalMeasurementOrder.Add(m);
                    }
                    m = new MeasurementOrderModel
                    {
                        Task = TaskTypes.BlockEnable,
                        Type = MeasurementType.Unknown,
                        SiPM = s[0],
                        StartModel = d
                    };
                    FinalMeasurementOrder.Add(m);
                    
                }

                switch (t)
                {
                    case MeasurementType.IVMeasurement:
                        taskType = TaskTypes.IV;
                        break;
                    case MeasurementType.ForwardResistanceMeasurement:
                        taskType = TaskTypes.ForwardResistance;
                        break;
                    case MeasurementType.DarkCurrentMeasurement:
                        taskType = TaskTypes.DarkCurrent;
                        break;
                    case MeasurementType.SPSMeasurement:
                        taskType = TaskTypes.SPS;
                        break;
                    case MeasurementType.DMMResistanceMeasurement:
                        taskType = TaskTypes.DMMResistance;
                        break;
                    case MeasurementType.TemperatureMeasurement:
                        taskType = TaskTypes.TemperatureMeasurement;
                        break;
                    default:
                        taskType = TaskTypes.Waiting;
                        break;
                }
                if (taskType == TaskTypes.IV)
                {
                    m = new MeasurementOrderModel
                    {
                        Task = TaskTypes.TemperatureMeasurement,
                        Type = MeasurementType.TemperatureMeasurement,
                        SiPM = s[0],
                        StartModel = d
                    };
                    FinalMeasurementOrder.Add(m);
                }
                m = new MeasurementOrderModel
                {
                    Task = taskType,
                    Type = t,
                    SiPM = s[0],
                    StartModel = d
                };
                FinalMeasurementOrder.Add(m);
                if (taskType == TaskTypes.IV)
                {
                    m = new MeasurementOrderModel
                    {
                        Task = TaskTypes.TemperatureMeasurement,
                        Type = MeasurementType.TemperatureMeasurement,
                        SiPM = s[0],
                        StartModel = d
                    };
                    FinalMeasurementOrder.Add(m);
                    m = new MeasurementOrderModel
                    {
                        Task = TaskTypes.Analysis,
                        Type = MeasurementType.Analysis,
                        SiPM = s[0],
                        StartModel = d
                    };
                    FinalMeasurementOrder.Add(m);
                }
                prevBlockChangeState = b;
            }

            if (FinalMeasurementOrder.Count > 0)
            {
                MeasurementOrderModel m = new MeasurementOrderModel
                {
                    Task = TaskTypes.BlockDisable,
                    Type = MeasurementType.Unknown,
                    SiPM = FinalMeasurementOrder.Last().SiPM,
                    StartModel = d
                };
                FinalMeasurementOrder.Add(m);
                CurrentMeasurementIndex = 0;
            }

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

        public bool GetNextTask(out MeasurementOrderModel nextMeasurement)
        {
            if (CurrentMeasurementIndex < FinalMeasurementOrder.Count)
            {
                nextMeasurement = FinalMeasurementOrder[CurrentMeasurementIndex];
                return true;
            }
            else
            {
                nextMeasurement = new MeasurementOrderModel();
                return false;
            }
        }

        /*
         * From the current index skip all those which belongs to the same block
         * This can be useful if DMM resistance measurement fails
         */
        public void SkipCurrentBlock()
        {
            MeasurementOrderModel current = FinalMeasurementOrder[CurrentMeasurementIndex];
            // Last measurement is a temperature measurement
            int lastIndex = FinalMeasurementOrder.FindLastIndex(m => m.SiPM.Block == current.SiPM.Block && m.Type == MeasurementType.TemperatureMeasurement);
            if (lastIndex < 0)
            {
                throw new IndexOutOfRangeException("Can not skip block");
            }
            CurrentMeasurementIndex = lastIndex + 1;
        }

        public void MarkCurrentTaskDone()
        {
            CurrentMeasurementIndex++;
        }

        public bool IsIVIterationAvailable()
        {
            return (CurrentIVMeasurementIndex < IVMeasurementOrder.Count && CurrentIVMeasurementIndex >= 0);
        }

        //Dark Current broke into two parts
        public bool IsDCIterationAvailable()
        {
            bool retVal = false;
            if (CurrentDCDCMeasurementIndex != CurrentDCLCMeasurementIndex)
            {
                retVal = true;
            }
            else if (CurrentDCDCMeasurementIndex < DCMeasurementOrder.Count && CurrentDCDCMeasurementIndex >= 0)
            {
                retVal = true;
            }
            else if (CurrentDCLCMeasurementIndex < DCMeasurementOrder.Count && CurrentDCLCMeasurementIndex >= 0)
            {
                retVal = true;
            }
            return retVal;
        }

        public bool IsFRIterationAvailable()
        {
            return (CurrentFRMeasurementIndex < FRMeasurementOrder.Count && CurrentFRMeasurementIndex >= 0);
        }

        public bool IsSPSterationAvailable()
        {
            return (CurrentSPSMeasurementIndex < SPSMeasurementOrder.Count && CurrentSPSMeasurementIndex >= 0);
        }

        public bool IsIterationAvailable()
        {
            return (IsIVIterationAvailable() || IsSPSterationAvailable() || IsDCIterationAvailable() || IsFRIterationAvailable());
        }


        /*
         * Pass back the necessary information to start the next round
         * New measurement order, second edition
         */
        public bool GetNextIterationDataNewOrderSE(out MeasurementType type, out object nextData, out List<CurrentSiPMModel> sipms, out bool isBlockChanging, bool goToNext = false)
        {
            sipms = new List<CurrentSiPMModel>();
            isBlockChanging = false;

            // backup original state
            bool currentMeasureDMMResistanceAtBegining = MeasureDMMResistanceAtBegining;
            int currentCurrendIVMeasurementIndex = CurrentIVMeasurementIndex;
            int currentCurrentBlockChangingIndex = CurrentBlockChangingIndex;
            MeasurementOrder currentCurrentMeasurementOrderCounter = CurrentMeasurementOrderCounter;

            bool retVal = IsIVIterationAvailable() || CurrentIVMeasurementIndex >= IVMeasurementOrder.Count;
            if (retVal)
            {
                if (CurrentIVMeasurementIndex == 0)
                {
                    if (CurrentMeasurementOrderCounter == 0)
                    {
                        CurrentBlockChangingIndex = CurrentIVMeasurementIndex;
                        isBlockChanging = true;
                        if (MeasureDMMResistanceAtBegining && isBlockChanging)
                        {
                            MeasureDMMResistanceAtBegining = false; //run at the beginning
                            type = MeasurementType.DMMResistanceMeasurement;
                            NIDMMStartModel startModel = new NIDMMStartModel();
                            startModel.Identifier = new MeasurementIdentifier(MeasurementType.DMMResistanceMeasurement);
                            startModel.DMMResistance = globalState.CurrentRun.DMMResistance;
                            nextData = startModel;

                            CurrentSiPMModel dmmSiPM = new CurrentSiPMModel(IVMeasurementOrder[CurrentIVMeasurementIndex].Block, 0, 0, 0);
                            sipms.Add(dmmSiPM); //dmm meas for next block x, 0, 0, 0

                            if (!goToNext)
                            {
                                MeasureDMMResistanceAtBegining = currentMeasureDMMResistanceAtBegining;
                            }

                            return true;
                        }
                    }
                    else
                    {
                        MeasureDMMResistanceAtBegining = true;
                    }
                    
                }
                else if (CurrentIVMeasurementIndex < IVMeasurementOrder.Count && CurrentIVMeasurementIndex > 0 && IVMeasurementOrder[CurrentIVMeasurementIndex].Block != IVMeasurementOrder[CurrentIVMeasurementIndex - 1].Block)
                {
                    if (CurrentMeasurementOrderCounter == MeasurementOrder.IV)
                    {
                        //finished all DC FR IV measurement
                        CurrentBlockChangingIndex = CurrentIVMeasurementIndex;
                        isBlockChanging = true;
                        if (MeasureDMMResistanceAtBegining && isBlockChanging)
                        {
                            MeasureDMMResistanceAtBegining = false; //run at the beginning
                            type = MeasurementType.DMMResistanceMeasurement;
                            NIDMMStartModel startModel = new NIDMMStartModel();
                            startModel.Identifier = new MeasurementIdentifier(MeasurementType.DMMResistanceMeasurement);
                            startModel.DMMResistance = globalState.CurrentRun.DMMResistance;
                            nextData = startModel;

                            CurrentSiPMModel dmmSiPM = new CurrentSiPMModel(IVMeasurementOrder[CurrentIVMeasurementIndex].Block, 0, 0, 0);
                            sipms.Add(dmmSiPM); //dmm meas for next block x, 0, 0, 0

                            if (!goToNext)
                            {
                                MeasureDMMResistanceAtBegining = currentMeasureDMMResistanceAtBegining;
                            }

                            return true;
                        }
                        CurrentMeasurementOrderCounter = MeasurementOrder.DCLC;
                    }
                    else if (CurrentMeasurementOrderCounter == MeasurementOrder.DCLC)
                    {
                        CurrentIVMeasurementIndex = CurrentBlockChangingIndex;
                        CurrentMeasurementOrderCounter = MeasurementOrder.FR;
                        MeasureDMMResistanceAtBegining = true;
                    }
                    else if (CurrentMeasurementOrderCounter == MeasurementOrder.FR)
                    {
                        CurrentIVMeasurementIndex = CurrentBlockChangingIndex;
                        CurrentMeasurementOrderCounter = MeasurementOrder.IV;
                    }
                }
                else if (CurrentIVMeasurementIndex >= IVMeasurementOrder.Count)
                {
                    if (CurrentMeasurementOrderCounter == MeasurementOrder.IV)
                    {
                        //finished all DC FR IV measurement
                        retVal = false;
                        type = MeasurementType.Unknown;
                        nextData = new object();
                        return retVal;
                    }
                    else if (CurrentMeasurementOrderCounter == MeasurementOrder.DCLC)
                    {
                        CurrentIVMeasurementIndex = CurrentBlockChangingIndex;
                        CurrentMeasurementOrderCounter = MeasurementOrder.FR;
                    }
                    else if (CurrentMeasurementOrderCounter == MeasurementOrder.FR)
                    {
                        CurrentIVMeasurementIndex = CurrentBlockChangingIndex;
                        CurrentMeasurementOrderCounter = MeasurementOrder.IV;
                    }
                }

                CurrentSiPMModel currentSiPM = IVMeasurementOrder[CurrentIVMeasurementIndex];
                if (CurrentMeasurementOrderCounter == MeasurementOrder.DCLC)
                {
                    type = MeasurementType.DarkCurrentMeasurement;
                    sipms.Add(currentSiPM); //return the next one

                    NIVoltageAndCurrentStartModel viStart = new NIVoltageAndCurrentStartModel();
                    viStart.Identifier = new MeasurementIdentifier(MeasurementType.DarkCurrentMeasurement);
                    viStart.MeasurementType = VoltageAndCurrentMeasurementTypes.LeakageCurrent;
                    nextData = viStart;

                    CurrentMeasurementOrderCounter++;
                }
                else if (CurrentMeasurementOrderCounter == MeasurementOrder.DCDC)
                {
                    type = MeasurementType.DarkCurrentMeasurement;
                    sipms.Add(currentSiPM); //return the next one

                    NIVoltageAndCurrentStartModel viStart = new NIVoltageAndCurrentStartModel();
                    viStart.Identifier = new MeasurementIdentifier(MeasurementType.DarkCurrentMeasurement);
                    viStart.MeasurementType = VoltageAndCurrentMeasurementTypes.DarkCurrent;
                    nextData = viStart;

                    CurrentIVMeasurementIndex++;
                    CurrentMeasurementOrderCounter--; //run DCLC for next one too
                }
                else if (CurrentMeasurementOrderCounter == MeasurementOrder.FR)
                {
                    type = MeasurementType.ForwardResistanceMeasurement;
                    sipms.Add(currentSiPM); //return the next one

                    NIVoltageAndCurrentStartModel viStart = new NIVoltageAndCurrentStartModel();
                    viStart.Identifier = new MeasurementIdentifier(MeasurementType.ForwardResistanceMeasurement);
                    viStart.MeasurementType = VoltageAndCurrentMeasurementTypes.ForwardResistance;
                    nextData = viStart;

                    CurrentIVMeasurementIndex++;
                }
                else if (CurrentMeasurementOrderCounter == MeasurementOrder.IV)
                {
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
                    else if (globalState.CurrentRun.IVVoltages.Count != 0)
                    {
                        ivStart.Voltages = globalState.CurrentRun.IVVoltages;
                    }
                    else
                    {
                        ivStart.OperatingVoltage = sipm.OperatingVoltage;
                    }
                    CurrentIVMeasurementIndex++; //increment IV index counter
                    nextData = ivStart;
                }
                else
                {
                    type = MeasurementType.Unknown;
                    nextData = new object();
                }
            }
            else
            {
                type = MeasurementType.Unknown;
                nextData = new object();
            }

            // restore original state
            if (!goToNext)
            {
                MeasureDMMResistanceAtBegining = currentMeasureDMMResistanceAtBegining;
                CurrentIVMeasurementIndex = currentCurrendIVMeasurementIndex;
                CurrentBlockChangingIndex = currentCurrentBlockChangingIndex;
                CurrentMeasurementOrderCounter = currentCurrentMeasurementOrderCounter;
            }

            return retVal;
        }



        /*
         * Pass back the necessary information to start the next round
         * New measurement order
         */
        public bool GetNextIterationDataNewOrder(out MeasurementType type, out object nextData, out List<CurrentSiPMModel> sipms)
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
                if (CurrentMeasurementOrderCounter == MeasurementOrder.DCLC)
                {
                    type = MeasurementType.DarkCurrentMeasurement;
                    sipms.Add(currentSiPM); //return the next one

                    NIVoltageAndCurrentStartModel viStart = new NIVoltageAndCurrentStartModel();
                    viStart.Identifier = new MeasurementIdentifier(MeasurementType.DarkCurrentMeasurement);
                    viStart.MeasurementType = VoltageAndCurrentMeasurementTypes.LeakageCurrent;
                    nextData = viStart;

                    CurrentMeasurementOrderCounter++;
                    CurrentMeasurementOrderCounter = (MeasurementOrder)((int)CurrentMeasurementOrderCounter % Enum.GetNames(typeof(MeasurementOrder)).Length);
                }
                else if (CurrentMeasurementOrderCounter == MeasurementOrder.DCDC)
                {
                    type = MeasurementType.DarkCurrentMeasurement;
                    sipms.Add(currentSiPM); //return the next one

                    NIVoltageAndCurrentStartModel viStart = new NIVoltageAndCurrentStartModel();
                    viStart.Identifier = new MeasurementIdentifier(MeasurementType.DarkCurrentMeasurement);
                    viStart.MeasurementType = VoltageAndCurrentMeasurementTypes.DarkCurrent;
                    nextData = viStart;

                    CurrentMeasurementOrderCounter++;
                    CurrentMeasurementOrderCounter = (MeasurementOrder)((int)CurrentMeasurementOrderCounter % Enum.GetNames(typeof(MeasurementOrder)).Length);
                }
                else if (CurrentMeasurementOrderCounter == MeasurementOrder.FR)
                {
                    type = MeasurementType.ForwardResistanceMeasurement;
                    sipms.Add(currentSiPM); //return the next one

                    NIVoltageAndCurrentStartModel viStart = new NIVoltageAndCurrentStartModel();
                    viStart.Identifier = new MeasurementIdentifier(MeasurementType.ForwardResistanceMeasurement);
                    viStart.MeasurementType = VoltageAndCurrentMeasurementTypes.ForwardResistance;
                    nextData = viStart;

                    CurrentMeasurementOrderCounter++;
                    CurrentMeasurementOrderCounter = (MeasurementOrder)((int)CurrentMeasurementOrderCounter % Enum.GetNames(typeof(MeasurementOrder)).Length);
                }
                else if (CurrentMeasurementOrderCounter == MeasurementOrder.IV)
                {
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
                    else if (globalState.CurrentRun.IVVoltages.Count != 0)
                    {
                        ivStart.Voltages = globalState.CurrentRun.IVVoltages;
                    }
                    else
                    {
                        ivStart.OperatingVoltage = sipm.OperatingVoltage;
                    }
                    CurrentIVMeasurementIndex++; //increment IV index counter
                    CurrentMeasurementOrderCounter++;
                    CurrentMeasurementOrderCounter = (MeasurementOrder)((int)CurrentMeasurementOrderCounter % Enum.GetNames(typeof(MeasurementOrder)).Length);
                    nextData = ivStart;
                }
                else
                {
                    type = MeasurementType.Unknown;
                    nextData = new object();
                }
            }
            else
            {
                type = MeasurementType.Unknown;
                nextData = new object();
            }

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

            bool retVal = IsDCIterationAvailable();
            if (retVal) //Add leakage and dc handling
            {
                CurrentSiPMModel currentSiPM;
                NIVoltageAndCurrentStartModel viStart;
                type = MeasurementType.DarkCurrentMeasurement;
                if (CurrentDCLCMeasurementIndex > CurrentDCDCMeasurementIndex)
                {
                    currentSiPM = DCMeasurementOrder[CurrentDCDCMeasurementIndex];
                    viStart = new NIVoltageAndCurrentStartModel();
                    viStart.MeasurementType = VoltageAndCurrentMeasurementTypes.DarkCurrent;
                    CurrentDCDCMeasurementIndex++;
                }
                else if (CurrentDCLCMeasurementIndex < DCMeasurementOrder.Count)
                {
                    currentSiPM = DCMeasurementOrder[CurrentDCLCMeasurementIndex];
                    viStart = new NIVoltageAndCurrentStartModel();
                    viStart.MeasurementType = VoltageAndCurrentMeasurementTypes.LeakageCurrent;
                    CurrentDCLCMeasurementIndex++;
                }
                else
                {
                    throw new IndexOutOfRangeException("Either DCLC or DCDC index is out of range");
                }
                sipms.Add(currentSiPM); //return the next one

                viStart.Identifier = new MeasurementIdentifier(MeasurementType.DarkCurrentMeasurement);
                nextData = viStart;
            }
            else if (retVal = IsFRIterationAvailable())
            {
                CurrentSiPMModel currentSiPM = FRMeasurementOrder[CurrentFRMeasurementIndex];
                type = MeasurementType.ForwardResistanceMeasurement;
                sipms.Add(currentSiPM); //return the next one

                NIVoltageAndCurrentStartModel viStart = new NIVoltageAndCurrentStartModel();
                viStart.Identifier = new MeasurementIdentifier(MeasurementType.ForwardResistanceMeasurement);
                viStart.MeasurementType = VoltageAndCurrentMeasurementTypes.ForwardResistance;

                CurrentFRMeasurementIndex++; //increment FR index counter
                nextData = viStart;
            }
            else if (retVal = IsIVIterationAvailable())
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
                else if (globalState.CurrentRun.IVVoltages.Count != 0)
                {
                    ivStart.Voltages = globalState.CurrentRun.IVVoltages;
                }
                else
                {
                    ivStart.OperatingVoltage = sipm.OperatingVoltage;
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

        public void FillEmptyCurrentRun(int BlockNum, int ModuleNum, int ArrayNum, int SiPMNum)
        {
            globalState.CurrentRun.Blocks = new List<Block>();

            for (int i = 0; i < BlockNum; i++)
            {
                Block block = new Block();
                for (int j = 0; j < ModuleNum; j++)
                {
                    Module module = new Module();
                    for (int k = 0; k < ArrayNum; k++)
                    {
                        Models.Array array = new Models.Array();
                        for (int l = 0; l < SiPMNum; l++)
                        {
                            SiPM sipm = new SiPM();
                            array.SiPMs.Add(sipm);
                        }
                        module.Arrays.Add(array);
                    }
                    block.Modules.Add(module);
                }
                globalState.CurrentRun.Blocks.Add(block);
            }
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
        //Shuffles IV, DC, FR lists
        private void ShuffleIVList(List<CurrentSiPMModel> list)
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
                //bool isValid = false;
                //int iterationCounter = 0;
                var newList = group.ToList();
                List<CurrentSiPMModel> newOrderedList = new List<CurrentSiPMModel>();
                if (newList.Count <= 0)
                {
                    break;
                }
                CurrentSiPMModel curr = new CurrentSiPMModel(newList[0].Block, newList[0].Module, newList[0].Array, 0);
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        curr.SiPM = j * 4 + i;
                        int index = newList.IndexOf(curr);
                        if (index > -1)
                        {
                            newOrderedList.Add(newList[index]);
                        }
                    }
                }
                list.AddRange(newOrderedList);
            }            
        }
    }
}

