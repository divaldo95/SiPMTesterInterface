using System;
using Newtonsoft.Json;

namespace SiPMTesterInterface.Helpers
{
	public static class JSONHelper
	{
        public static T ReadJsonFile<T>(string filePath)
        {
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("JSON file not found.", filePath);
            }

            // Read the JSON file
            string jsonText = File.ReadAllText(filePath);

            // Deserialize the JSON into the specified type
            T jsonObject = JsonConvert.DeserializeObject<T>(jsonText);

            return jsonObject;
        }
    }
}

