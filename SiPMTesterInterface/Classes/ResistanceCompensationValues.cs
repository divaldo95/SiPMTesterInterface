using System;
using Newtonsoft.Json;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Classes
{
	public class ResistanceCompensationValues
	{
        private readonly Dictionary<(int Block, int Module, int Array), double> _resistanceDictionary;

        // Constructor to load from a JSON file
        public ResistanceCompensationValues(string jsonFilePath)
        {
            var jsonData = File.ReadAllText(jsonFilePath);
            var resistanceList = JsonConvert.DeserializeObject<List<ResistanceModel>>(jsonData);

            _resistanceDictionary = resistanceList.ToDictionary(
                r => (r.Block, r.Module, r.Array),
                r => r.Resistance);
        }

        // Constructor to load from IConfiguration and generate all array resistances
        public ResistanceCompensationValues(IConfiguration configuration)
        {
            _resistanceDictionary = new Dictionary<(int Block, int Module, int Array), double>();

            // Assuming the configuration section is named "Resistances"
            var resistanceList = configuration.GetSection("Resistances").Get<List<ResistanceModel>>();

            // Create entries for all arrays
            for (int block = 0; block < 5; block++)
            {
                for (int module = 0; module < 2; module++)
                {
                    for (int array = 0; array < 4; array++)
                    {
                        var resistance = resistanceList.FirstOrDefault(r => r.Block == block && r.Module == module && r.Array == array)?.Resistance;
                        _resistanceDictionary[(block, module, array)] = resistance ?? default; // Assign default (0.0) if not found
                    }
                }
            }
        }

        // Method to get resistance value
        public double? GetResistance(int blockId, int moduleId, int arrayId)
        {
            if (_resistanceDictionary.TryGetValue((blockId, moduleId, arrayId), out var resistance))
            {
                return resistance;
            }

            return null;
        }
    }
}

