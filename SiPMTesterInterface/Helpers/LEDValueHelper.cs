using System;
using Newtonsoft.Json;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Helpers
{
	public static class LEDValueHelper
	{
		public static int GetPulserValue(int baseValue, int block = 0, int module = 0, int array = 0, int sipm = 0)
		{
			//placeholder for LED pulser value calculation
			return baseValue;
		}

        public static List<SiPMPulserValue> GenerateDefaultIVPulserData(int blocks, int modules, int arrays, int sipms, int defaultPulserValue)
        {
            var sensorDataList = new List<SiPMPulserValue>();

            for (int block = 0; block < blocks; block++)
            {
                for (int module = 0; module < modules; module++)
                {
                    for (int array = 0; array < arrays; array++)
                    {
                        for (int sipm = 0; sipm < sipms; sipm++)
                        {
                            sensorDataList.Add(new SiPMPulserValue
                            {
                                SiPM = new CurrentSiPMModel
                                {
                                    Block = block,
                                    Module = module,
                                    Array = array,
                                    SiPM = sipm
                                },
                                PulserValue = defaultPulserValue
                            });
                        }
                    }
                }
            }

            return sensorDataList;
        }

        public static List<SiPMPulserValue> ReadAndUpdateFromJsonFile(string fileName, List<SiPMPulserValue> defaultData)
        {
            if (File.Exists(fileName))
            {
                // Read the JSON file content
                string json = File.ReadAllText(fileName);

                // Deserialize the JSON content to a list of SensorData objects
                var jsonData = JsonConvert.DeserializeObject<List<SiPMPulserValue>>(json);

                // Update the default data with the values from the JSON file
                foreach (var jsonItem in jsonData)
                {
                    var match = defaultData.FirstOrDefault(d =>
                        d.SiPM.Block == jsonItem.SiPM.Block &&
                        d.SiPM.Module == jsonItem.SiPM.Module &&
                        d.SiPM.Array == jsonItem.SiPM.Array &&
                        d.SiPM.SiPM == jsonItem.SiPM.SiPM);

                    if (match != null)
                    {
                        match.PulserValue = jsonItem.PulserValue;
                    }
                }
            }
            else
            {
                throw new FileNotFoundException($"The file {fileName} does not exist.");
            }

            return defaultData;
        }

        public static SiPMPulserValue GetPulserValueForSiPM(CurrentSiPMModel sipm, List<SiPMPulserValue> defaultData)
        {
            var match = defaultData.FirstOrDefault(d =>
                        d.SiPM.Block == sipm.Block &&
                        d.SiPM.Module == sipm.Module &&
                        d.SiPM.Array == sipm.Array &&
                        d.SiPM.SiPM == sipm.SiPM, null);

            if (match != null)
            {
                return match;
            }
            else
            {
                throw new ArgumentException("SiPM pulser value not found");
            }
        }

        public static void UpdatePulserValueForSiPM(int block, int module, int array, int sipm, int newPulser, List<SiPMPulserValue> defaultData)
        {
            var match = defaultData.FirstOrDefault(d =>
                        d.SiPM.Block == block &&
                        d.SiPM.Module == module &&
                        d.SiPM.Array == array &&
                        d.SiPM.SiPM == sipm, null);
            
            if (match != null)
            {
                var index = defaultData.IndexOf(match);
                defaultData[index].PulserValue = newPulser;
            }
            else
            {
                throw new ArgumentException("SiPM pulser value not found");
            }
        }

        public static void UpdatePulserValueForSiPM(CurrentSiPMModel sipm, int newPulser, List<SiPMPulserValue> defaultData)
        {
            UpdatePulserValueForSiPM(sipm.Block, sipm.Module, sipm.Array, sipm.SiPM, newPulser, defaultData);
        }
    }
}

