using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
    public static class Extensions
    {
        public static T[] Populate<T>(this T[] array, Func<T> provider)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = provider();
            }
            return array;
        }
    }

	public class ServiceStateHandler
	{
        private int BlockNum;
        private int ArrayNum;
        private int ModuleNum;
        private int SiPMNum;

        private int bufferSize = 5000;

        private CurrentMeasurementDataModel[] AllSiPMsData; //store data for every single SiPM (flattened)
        private PulserValues[] pulserValues; //store pulser values for every single SiPM (flattened)
        public Queue<DMMResistanceMeasurementResponseModel> DMMResistances { get; private set; }

        //Save current measurement results here in a flattened array
        public ServiceStateHandler(int blockNum = 2, int moduleNum = 2, int arrayNum = 4, int sipmNum = 16)
		{
            BlockNum = blockNum;
            ArrayNum = arrayNum;
            ModuleNum = moduleNum;
            SiPMNum = sipmNum;

            int allNum = BlockNum * ArrayNum * ModuleNum * SiPMNum;

            //init to default
            AllSiPMsData = new CurrentMeasurementDataModel[allNum].Populate(() => new CurrentMeasurementDataModel());
            pulserValues = new PulserValues[allNum].Populate(() => new PulserValues());
            DMMResistances = new Queue<DMMResistanceMeasurementResponseModel>();
		}

        public string GetSiPMMeasurementStatesJSON()
        {
            if (AllSiPMsData == null)
            {
                throw new NullReferenceException("Measurement not started yet");
            }
            // Project each group into the desired JSON structure
            var result = new
            {
                Blocks = Enumerable.Range(0, BlockNum).Select(blockIndex =>
                new
                {
                    Modules = Enumerable.Range(0, ModuleNum).Select(moduleIndex =>
                        new
                        {
                            Arrays = Enumerable.Range(0, ArrayNum).Select(arrayIndex =>
                                new
                                {
                                    SiPMs = Enumerable.Range(0, SiPMNum).Select(sipmIndex =>
                                        new
                                        {
                                            IVMeasurementDone = AllSiPMsData[blockIndex * 2 * 4 * 16 + moduleIndex * 4 * 16 + arrayIndex * 16 + sipmIndex]?.IsIVDone ?? false,
                                            SPSMeasurementDone = AllSiPMsData[blockIndex * 2 * 4 * 16 + moduleIndex * 4 * 16 + arrayIndex * 16 + sipmIndex]?.IsSPSDone ?? false,
                                            //Analyse and update these values
                                            IVTimes = new
                                            {
                                                StartTimestamp = AllSiPMsData[blockIndex * 2 * 4 * 16 + moduleIndex * 4 * 16 + arrayIndex * 16 + sipmIndex]?.IVResult.StartTimestamp,
                                                EndTimestamp = AllSiPMsData[blockIndex * 2 * 4 * 16 + moduleIndex * 4 * 16 + arrayIndex * 16 + sipmIndex]?.IVResult.EndTimestamp
                                            },
                                            IVAnalysationResult = AllSiPMsData[blockIndex * 2 * 4 * 16 + moduleIndex * 4 * 16 + arrayIndex * 16 + sipmIndex].IVResult.AnalysationResult,
                                            SPSResult = new
                                            {
                                                isOK = false,
                                                Gain = 0.0
                                            }
                                        })
                                })
                        })
                }),
            };
                
            return Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        }

        public void AppendDMMResistanceMeasurement(DMMResistanceMeasurementResponseModel newRes)
        {
            // If buffer is full, dequeue the oldest element
            if (DMMResistances.Count == bufferSize)
            {
                DMMResistances.Dequeue();
            }

            // Add the new temperature array to the buffer
            DMMResistances.Enqueue(newRes);
        }

        public PulserValues GetSiPMPulserValues(int block, int module, int array, int sipm)
        {
            return pulserValues[GetPulserArrayIndex(block, module, array, sipm)];
        }

        public void SetIVPulserValue(int block, int module, int array, int sipm, int value)
        {
            pulserValues[GetPulserArrayIndex(block, module, array, sipm)].IVPulser = value;
        }

        public void SetSPSPulserValue(int block, int module, int array, int sipm, int value)
        {
            pulserValues[GetPulserArrayIndex(block, module, array, sipm)].SPSPulser = value;
        }

        public CurrentMeasurementDataModel GetSiPMMeasurementData(int block, int module, int array, int sipm)
        {
            int arrayIndex = GetPulserArrayIndex(block, module, array, sipm);
            if (AllSiPMsData.Length < arrayIndex)
            {
                throw new ArgumentOutOfRangeException("Measurement not started or index out of bounds");
            }
            return AllSiPMsData[arrayIndex];
        }

        public bool GetSiPMIVMeasurementData(MeasurementIdentifier id, out CurrentMeasurementDataModel sipmData)
        {
            if (AllSiPMsData == null)
            {
                sipmData = new CurrentMeasurementDataModel();
                return false;
            }
            CurrentMeasurementDataModel? sipmMeasData = AllSiPMsData.FirstOrDefault(model => model.IVMeasurementID.Equals(id));

            if (sipmMeasData != null)
            {
                sipmData = sipmMeasData;
                return true;
            }
            else
            {
                sipmData = new CurrentMeasurementDataModel();
                return false;
            }
        }

        public void SetIVMeasurementData(int block, int module, int array, int sipm, IVMeasurementResponseModel result)
        {
            AllSiPMsData[GetPulserArrayIndex(block, module, array, sipm)].IVResult = result;
        }

        public void SetIVMeasurementIdentifier(int block, int module, int array, int sipm, MeasurementIdentifier id)
        {
            AllSiPMsData[GetPulserArrayIndex(block, module, array, sipm)].IVMeasurementID = id;
        }

        public void SetSPSMeasurementIdentifier(int block, int module, int array, int sipm, MeasurementIdentifier id)
        {
            AllSiPMsData[GetPulserArrayIndex(block, module, array, sipm)].SPSMeasurementID = id;
        }

        private int GetPulserArrayIndex(int block, int module, int array, int sipm)
        {
            if (block < 0 || block >= BlockNum)
            {
                throw new ArgumentOutOfRangeException("Block is out of range while getting a pulser value");
            }
            if (module < 0 || module >= ModuleNum)
            {
                throw new ArgumentOutOfRangeException("Module is out of range while getting a pulser value");
            }
            if (array < 0 || array >= ArrayNum)
            {
                throw new ArgumentOutOfRangeException("Array is out of range while getting a pulser value");
            }
            if (sipm < 0 || sipm >= SiPMNum)
            {
                throw new ArgumentOutOfRangeException("SiPM is out of range while getting a pulser value");
            }
            int sipmOffset = sipm;
            int arrayOffset = SiPMNum;
            int moduleOffset = SiPMNum * ArrayNum;
            int blockOffset = SiPMNum * ArrayNum * ModuleNum;
            return block * blockOffset + module * moduleOffset + array * arrayOffset + sipmOffset;
        }
    }
}

