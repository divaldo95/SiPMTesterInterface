using System;
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
        private CurrentMeasurementDataModel[] AllSiPMsData; //store data for every single SiPM (flattened)

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
		}

        public CurrentMeasurementDataModel GetSiPMMeasurementData(int block, int module, int array, int sipm)
        {
            return AllSiPMsData[GetPulserArrayIndex(block, module, array, sipm)];
        }

        public bool GetSiPMIVMeasurementData(MeasurementIdentifier id, out CurrentMeasurementDataModel sipmData)
        {
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

