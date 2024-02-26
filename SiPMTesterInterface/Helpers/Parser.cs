using System;
using System.Text.Json;

namespace SiPMTesterInterface.Helpers
{
	public static class Parser
	{
        public static bool ValidateAndGetJSON(string jsonString, out JsonDocument? doc)
        {
            try
            {
                // Attempt to parse the JSON string
                doc = JsonDocument.Parse(jsonString);

                // If parsing succeeds, the JSON is valid
                return true;
            }
            catch (JsonException)
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

