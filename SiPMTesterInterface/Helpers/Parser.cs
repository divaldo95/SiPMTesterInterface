using System;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SiPMTesterInterface.Helpers
{
	public static class Parser
	{
        public static bool ValidateAndGetJSON(string jsonString, out JObject doc)
        {
            try
            {
                // Attempt to parse the JSON string
                doc = JObject.Parse(jsonString);

                // If parsing succeeds, the JSON is valid
                return true;
            }
            catch (JsonReaderException)
            {
                doc = null;
                // If an exception is thrown during parsing, the JSON is not valid
                return false;
            }
        }

        public static bool ParseMeasurementStatus(string status, out string sender, out JObject document)
        {
            int startIndex = status.IndexOf('{');
            int endIndex = status.LastIndexOf('}');
            if (startIndex == -1 || endIndex == -1)
            {
                Console.WriteLine("No JSON string and sender found in the input.");
                sender = "";
                document = null;
                return false;
            }
            sender = status.Substring(0, startIndex - 1); //not adding +1 because it is the comma, deleting last ':'
                                                      // Extract the substring containing the JSON string
            string jsonString = status.Substring(startIndex, endIndex - startIndex + 1);
            if (ValidateAndGetJSON(jsonString, out document))
            {
                return true;
            }
            document = null;
            return false;
        }

        public static bool JObject2JSON<T>(JObject obj, out T doc, out string error)
        {
            try
            {
                doc = obj.ToObject<T>();
                error = default;
                return true;
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine($"Error deserializing JObject: {ex.Message}");
                doc = default;
                error = ex.Message;
                return false;
            }
        }

        public static bool ParseJSON<T>(string status, out string sender, out string error, out T document)
        {
            JObject obj;
            if (!ParseMeasurementStatus(status, out sender, out obj))
            {
                sender = default;
                document = default;
                error = default;
                return false;
            }
            return JObject2JSON(obj, out document, out error);

        }

        public static bool ValidateAndGetJSON(string jsonString, out JsonDocument? doc)
        {
            try
            {
                // Attempt to parse the JSON string
                doc = JsonDocument.Parse(jsonString);

                // If parsing succeeds, the JSON is valid
                return true;
            }
            catch (System.Text.Json.JsonException)
            {
                doc = null;
                // If an exception is thrown during parsing, the JSON is not valid
                return false;
            }
        }

        public static bool ParseMeasurementStatus(string status, out string sender, out JsonDocument? document)
		{
            int startIndex = status.IndexOf('{');
            int endIndex = status.LastIndexOf('}');
            if (startIndex == -1 && endIndex == -1)
            {
                Console.WriteLine("No JSON string and sender found in the input.");
                sender = "";
                document = null;
                return false;
            }
            sender = status.Substring(0, startIndex); //not adding +1 because it is the comma
            // Extract the substring containing the JSON string
            string jsonString = status.Substring(startIndex, endIndex - startIndex + 1);
            if (ValidateAndGetJSON(jsonString, out document))
            {
                return true;
            }
            document = null;
            return false;
        }
	}
}

