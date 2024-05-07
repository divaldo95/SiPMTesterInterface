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
    }
}

