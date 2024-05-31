using System.Diagnostics;
using System.Reflection.PortableExecutable;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.ClientApp.Services;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;
using SiPMTesterInterface.Models;

namespace InterfaceTests;



[TestClass]
public class OrchestratorUnitTest
{
    [TestMethod]
    public void SPSOutputTest()
    {
        string inJson = InputJSONs.inputJSON1;
        MeasurementStartModel? startModel;
        bool success = Parser.String2JSON(inJson, out startModel);
        MeasurementOrchestrator orchestrator = new MeasurementOrchestrator();
        if (!success || startModel == null)
        {
            Debug.Fail("Invalid input json string");
        }
        orchestrator.PrepareMeasurement(startModel);

        Console.WriteLine("Iterations:");
        MeasurementType Type;
        object nextMeasurementData;
        List<CurrentSiPMModel> sipms;

        while (orchestrator.GetNextIterationData(out Type, out nextMeasurementData, out sipms))
        {
            if (Type == MeasurementType.DMMResistanceMeasurement)
            {
                Console.WriteLine("DMM Measurement");
                continue;
            }

            else if (Type == MeasurementType.IVMeasurement)
            {
                if (sipms.Count != 1)
                {
                    Debug.Fail($"Can not measure more than one SiPM at a time for IV");
                }
            }
            else if (Type == MeasurementType.SPSMeasurement) 
            {
                if (sipms.Count < 1 || sipms.Count > 8)
                {
                    Debug.Fail($"SiPM count error on SPS measurement ({sipms.Count})");
                }
            }
            else //end of measurement
            {
                Debug.Fail($"Unknown measurement type");
            }

            Console.WriteLine($"{Type}");
            for (int i = 0; i < sipms.Count; i++)
            {
                Console.WriteLine($"{sipms[i]}");
            }
            Console.WriteLine("----------------------------------------------------------");
        }
    }
}