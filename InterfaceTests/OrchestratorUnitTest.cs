using System.Diagnostics;
using System.Reflection.PortableExecutable;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
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
    public void FinalMeasurementOrderTest()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger<MeasurementOrchestrator> logger = factory.CreateLogger<MeasurementOrchestrator>();

        string inJson = InputJSONs.inputJSON3;
        MeasurementStartModel? startModel;
        bool success = Parser.String2JSON(inJson, out startModel);
        MeasurementOrchestrator orchestrator = new MeasurementOrchestrator(logger);
        if (!success || startModel == null)
        {
            Debug.Fail("Invalid input json string");
        }
        orchestrator.PrepareMeasurement(startModel);

        MeasurementOrderModel m;

        while (orchestrator.GetNextTask(out m))
        {
            Console.WriteLine(m);
            // Simulate a failed DMM measurement
            if (m.SiPM.Block == 0 && m.Type == MeasurementType.DMMResistanceMeasurement)
            {
                Console.WriteLine("Simulating failed DMM resistance measurement");
                orchestrator.SkipCurrentBlock();
            }
            else
            {
                orchestrator.MarkCurrentTaskDone();
            }
        }
    }

    [TestMethod]
    public void SPSOutputTest()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger<MeasurementOrchestrator> logger = factory.CreateLogger<MeasurementOrchestrator>();

        string inJson = InputJSONs.inputJSON1;
        MeasurementStartModel? startModel;
        bool success = Parser.String2JSON(inJson, out startModel);
        MeasurementOrchestrator orchestrator = new MeasurementOrchestrator(logger);
        if (!success || startModel == null)
        {
            Debug.Fail("Invalid input json string");
        }
        orchestrator.PrepareMeasurement(startModel);

        Console.WriteLine("Iterations:");
        MeasurementType Type;
        object nextMeasurementData;
        List<CurrentSiPMModel> sipms;

        int nextBlock = -1;
        int nextModule = -1;
        MeasurementType blockChangingType;

        bool isChanging = false;

        while (true)
        {
            if (orchestrator.GetNextIterationDataNewOrderSE(out Type, out nextMeasurementData, out sipms, out isChanging, false))
            {
                if (isChanging)
                {
                    Console.WriteLine($"Block is changing to {Type}");
                    if (sipms.Count > 0)
                        Console.WriteLine($"Block number is {sipms[0].Block}");
                }
                
            }

            if (orchestrator.GetNextIterationDataNewOrderSE(out Type, out nextMeasurementData, out sipms, out isChanging, false))
            {
                if (isChanging)
                {
                    Console.WriteLine($"Block is changing to {Type}");
                    if (sipms.Count > 0)
                        Console.WriteLine($"Block number is {sipms[0].Block}");
                }

            }

            if (orchestrator.GetNextIterationDataNewOrderSE(out Type, out nextMeasurementData, out sipms, out isChanging, false))
            {
                if (isChanging)
                {
                    Console.WriteLine($"Block is changing to {Type}");
                    if (sipms.Count > 0)
                        Console.WriteLine($"Block number is {sipms[0].Block}");
                }

            }

            if (orchestrator.GetNextIterationDataNewOrderSE(out Type, out nextMeasurementData, out sipms, out _, true))
            {
                string additionalData = "";
                if (Type == MeasurementType.DMMResistanceMeasurement)
                {
                    Console.WriteLine("DMM Measurement");
                    if (sipms.Count != 1)
                    {
                        Debug.Fail($"Can not measure more than one block resistance a time");
                    }
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
                else if (Type == MeasurementType.ForwardResistanceMeasurement)
                {
                    if (sipms.Count != 1)
                    {
                        Debug.Fail($"SiPM count error on FR measurement ({sipms.Count})");
                    }
                }
                else if (Type == MeasurementType.DarkCurrentMeasurement)
                {
                    var dcd = nextMeasurementData as NIVoltageAndCurrentStartModel;
                    if (sipms.Count != 1)
                    {
                        Debug.Fail($"SiPM count error on DC measurement ({sipms.Count})");
                    }
                    additionalData = $"| {dcd.MeasurementType}";
                }
                else //end of measurement
                {
                    Debug.Fail($"Unknown measurement type");
                }

                Console.WriteLine($"{Type}");
                for (int i = 0; i < sipms.Count; i++)
                {
                    Console.WriteLine($"{sipms[i]} {additionalData}");
                }
                Console.WriteLine("----------------------------------------------------------");
            }
            else
            {
                break;
            }
        }
    }
}