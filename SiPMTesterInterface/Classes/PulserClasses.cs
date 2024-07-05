using System;
using System.Collections;
using Newtonsoft.Json;
using SiPMTesterInterface.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SiPMTesterInterface.Classes
{
    public class PulserSiPM
    {
        public int PulserValue { get; set; }
    }

    public class PulserArray
    {
        public List<PulserSiPM> SiPMs { get; set; }
        public int LEDPulserOffset { get; set; } = 0;
    }

    public class PulserModule
    {
        public List<PulserArray> Arrays { get; set; }
    }

    public class PulserBlock
    {
        public List<PulserModule> Modules { get; set; }
    }

    public class PulserUpdate
    {
        public int BlockIndex { get; set; }
        public int ModuleIndex { get; set; }
        public int ArrayIndex { get; set; }
        public int SiPMIndex { get; set; }
        public int PulserValue { get; set; }
    }

    public class LEDPulserOffsetUpdate
    {
        public int BlockIndex { get; set; }
        public int ModuleIndex { get; set; }
        public int ArrayIndex { get; set; }
        public int LEDPulserOffset { get; set; }
    }

    public class LEDPulserData
    {
        public List<PulserBlock> Blocks { get; set; } = new List<PulserBlock>();

        public LEDPulserData()
        {

        }

        public int GetPulserValue(int blockIndex, int moduleIndex, int arrayIndex, int sipmIndex, bool includeOffset = true)
        {
            int pulserValue = Blocks[blockIndex]
                        .Modules[moduleIndex]
                        .Arrays[arrayIndex]
                        .SiPMs[sipmIndex]
                        .PulserValue;

            if (includeOffset)
            {
                int offset = Blocks[blockIndex]
                                 .Modules[moduleIndex]
                                 .Arrays[arrayIndex]
                                 .LEDPulserOffset;
                pulserValue += offset;
            }

            return pulserValue;
        }

        public int GetPulserValue(CurrentSiPMModel sipm, bool includeOffset = true)
        {
            return GetPulserValue(sipm.Block, sipm.Module, sipm.Array, sipm.SiPM, includeOffset);
        }

        public void SetPulserValue(int blockIndex, int moduleIndex, int arrayIndex, int sipmIndex, int newPulserValue)
        {
            Blocks[blockIndex]
                .Modules[moduleIndex]
                .Arrays[arrayIndex]
                .SiPMs[sipmIndex]
                .PulserValue = newPulserValue;
        }

        public void SetPulserValue(CurrentSiPMModel sipm, int newPulserValue)
        {
            SetPulserValue(sipm.Block, sipm.Module, sipm.Array, sipm.SiPM, newPulserValue);
        }

        public void SetArrayOffset(CurrentSiPMModel sipm, int newOffsetValue)
        {
            SetArrayOffset(sipm.Block, sipm.Module, sipm.Array, newOffsetValue);
        }

        public void SetArrayOffset(int blockIndex, int moduleIndex, int arrayIndex, int newOffsetValue)
        {
            Blocks[blockIndex]
                .Modules[moduleIndex]
                .Arrays[arrayIndex]
                .LEDPulserOffset = newOffsetValue;
        }

        public void Init(int numberOfBlocks, int numberOfModules, int numberOfArrays, int numberOfSiPMs, int defaultValue)
        {
            FillDataStructure(numberOfBlocks, numberOfModules, numberOfArrays, numberOfSiPMs, defaultValue);
        }

        public void ApplyUpdatesFromJson(string jsonFilePath)
        {
            var jsonData = File.ReadAllText(jsonFilePath);
            var updates = JsonConvert.DeserializeObject<List<PulserUpdate>>(jsonData);

            foreach (var update in updates)
            {
                Blocks[update.BlockIndex]
                    .Modules[update.ModuleIndex]
                    .Arrays[update.ArrayIndex]
                    .SiPMs[update.SiPMIndex]
                    .PulserValue = update.PulserValue;
            }
        }

        public void ApplyLEDPulserOffsetUpdatesFromJson(string jsonFilePath)
        {
            var jsonData = File.ReadAllText(jsonFilePath);
            var updates = JsonConvert.DeserializeObject<List<LEDPulserOffsetUpdate>>(jsonData);

            foreach (var update in updates)
            {
                Blocks[update.BlockIndex]
                    .Modules[update.ModuleIndex]
                    .Arrays[update.ArrayIndex]
                    .LEDPulserOffset = update.LEDPulserOffset;
            }
        }

        public void FillDataStructure(int numberOfBlocks, int numberOfModules, int numberOfArrays, int numberOfSiPMs, int defaultValue)
        {
            Blocks = new List<PulserBlock>();

            for (int b = 0; b < numberOfBlocks; b++)
            {
                var block = new PulserBlock
                {
                    Modules = new List<PulserModule>()
                };

                for (int m = 0; m < numberOfModules; m++)
                {
                    var module = new PulserModule
                    {
                        Arrays = new List<PulserArray>()
                    };

                    for (int a = 0; a < numberOfArrays; a++)
                    {
                        var array = new PulserArray
                        {
                            SiPMs = new List<PulserSiPM>(),
                            LEDPulserOffset = 10 // default LEDPulserOffset value
                        };

                        for (int s = 0; s < numberOfSiPMs; s++)
                        {
                            var sipm = new PulserSiPM
                            {
                                PulserValue = defaultValue
                            };
                            array.SiPMs.Add(sipm);
                        }

                        module.Arrays.Add(array);
                    }

                    block.Modules.Add(module);
                }

                Blocks.Add(block);
            }
        }

        public static bool GetBit(byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        public class APSUDiagResponse
        {
            public byte i2cReadErrorFlags { get; set; } // bit 0-5 block 0-5
            public byte state { get; set; } // 0 off 1 on  //bit 0-5 block 0-5
            public byte[] errorFlags { get; set; } = new byte[6]; // bit 0 undervoltage bit1 overvoltage bit2 undercurrent bit3 overcurrent bit4 !Power_good
            public float[] AVoltage { get; set; } = new float[6];
            public float[] ACurrent { get; set; } = new float[6];

            public APSUDiagResponse()
            {

            }

            public bool GetAPSUState(int block)
            {
                if (block > 5 || block < 0)
                {
                    throw new ArgumentOutOfRangeException("Block must be between 0 and 5");
                }
                return GetBit(state, block);
            }
        }

    }

    
}

