using System;
using Newtonsoft.Json;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Helpers
{
	public static class FileOperationHelper
	{
		public static void SaveIVResult(CurrentMeasurementDataModel data, string folderName)
		{
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			string outPath = Path.Combine(baseDir, folderName);
			string fileName = $"IV_{data.SiPMLocation.Block}_{data.SiPMLocation.Module}_{data.SiPMLocation.Array}_{data.SiPMLocation.SiPM}.json";
			if (!Directory.Exists(outPath))
			{
				Directory.CreateDirectory(outPath);
				Console.WriteLine($"Directory ({outPath}) created");
			}
            using (StreamWriter file = File.CreateText(outPath + "/" + fileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, data);
                Console.WriteLine($"File ({outPath + fileName}) created");
            }
        }

        public static void SaveDMMResult(List<DMMResistanceMeasurementResponseModel> data, string folderName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string outPath = Path.Combine(baseDir, folderName);
            string fileName = $"DMMResistance.json";
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
                Console.WriteLine($"Directory ({outPath}) created");
            }
            using (StreamWriter file = File.CreateText(outPath + "/" + fileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, data);
                Console.WriteLine($"File ({outPath + fileName}) created");
            }
        }

        public static void CreateOrAppendToFileDMMMeasurement(string basePath, string fileName, DMMResistanceMeasurementResponseModel data)
        {
            List<DMMResistanceMeasurementResponseModel>? dataList;
            string outFile = Path.Combine(basePath, fileName);
            Directory.CreateDirectory(basePath);

            // Deserialize existing data from the file, or create a new list if the file doesn't exist
            if (File.Exists(outFile))
            {
                string json = File.ReadAllText(outFile);
                dataList = JsonConvert.DeserializeObject<List<DMMResistanceMeasurementResponseModel>>(json);
                if (dataList == null)
                {
                    dataList = new List<DMMResistanceMeasurementResponseModel>();
                }
            }
            else
            {
                dataList = new List<DMMResistanceMeasurementResponseModel>();
            }

            // Add the new measurement data to the list
            dataList.Add(data);

            // Serialize the updated list to JSON
            string updatedJson = JsonConvert.SerializeObject(dataList, Formatting.Indented);

            // Write the JSON string back to the file
            File.WriteAllText(outFile, updatedJson);
        }
    }
}

