using System;
using System.Runtime.InteropServices;
using SiPMTesterInterface.AnalysisModels;
using SiPMTesterInterface.Helpers;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Libraries
{
    public class RootIVAnalyser : IDisposable
    {
        const string LibraryName = "RootIVAnalyser";

        [DllImport(LibraryName, EntryPoint = "RIVA_Class_Create", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr RIVA_Class_Create();
        [DllImport(LibraryName, EntryPoint = "RIVA_Class_Delete", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RIVA_Class_Delete(IntPtr ivAnalyser);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr RIVA_Class_AnalyseIV(IntPtr ivAnalyser,
                                                    SiPMData data,
                                                    AnalysisTypes method,
                                                    double temperatureToCompensate,
                                                    bool savePlots,
                                                    string outBasePath,
                                                    string filePrefix);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RIVA_Class_GetResults(IntPtr ivAnalyser,
                                                         out double rawVbr,
                                                         out double compVbr,
                                                         out double cs);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RIVA_Class_SetProperties(IntPtr ivAnalyser, AnalysisProperties properties);

        private IntPtr ivAnalyser;
        private bool disposedValue;

        public RootIVAnalyser()
        {
            ivAnalyser = RIVA_Class_Create();
        }

        public void AnalyseIV(SiPMData data, AnalysisTypes type, double temperatureToCompensate, bool savePlots, string outBasePath, string filePrefix)
        {
            RIVA_Class_AnalyseIV(ivAnalyser, data, type, temperatureToCompensate, savePlots, outBasePath, filePrefix);
        }

        public void GetResult(out double RawBreakdownVoltage, out double CompensatedBreakdownVoltage, out double ChiSquare)
        {
            RIVA_Class_GetResults(ivAnalyser, out RawBreakdownVoltage, out CompensatedBreakdownVoltage, out ChiSquare);
        }

        public void SetProperties(AnalysisProperties properties)
        {
            RIVA_Class_SetProperties(ivAnalyser, properties);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    RIVA_Class_Delete(ivAnalyser);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~RootIVAnalyser()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static void Analyse(CurrentMeasurementDataModel c, string outputPath, AnalysisProperties? properties = null)
        {
            //Calculate DMM currents

            Console.WriteLine("Starting analysis...");
            RootIVAnalyser iv = new RootIVAnalyser();

            if (properties != null)
            {
                iv.SetProperties((AnalysisProperties)properties); //null check already done
            }

            double vbr;
            double cvbr;
            double cs;

            double[] voltagesArray = c.IVResult.DMMVoltage.ToArray();
            double[] currentsArray = c.IVResult.SMUCurrent.ToArray();

            //compensate current
            if (c.DMMResistanceResult.Resistance > 0)
            {
                for (int i = 0; i < voltagesArray.Length; i++)
                {
                    double current = voltagesArray[i] / c.DMMResistanceResult.Resistance;
                    currentsArray[i] = currentsArray[i] - current;
                    if (currentsArray[i] < 0)
                    {
                        
                        Console.WriteLine($"Negative current ({currentsArray[i].ToString("0.00")}). Consider increasing the DMM resistance compensation percentage");
                        currentsArray[i] = 0;
                    }
                }
            }
            
            // Pin the arrays to get their pointers
            GCHandle voltagesHandle = GCHandle.Alloc(voltagesArray, GCHandleType.Pinned);
            GCHandle currentsHandle = GCHandle.Alloc(currentsArray, GCHandleType.Pinned);

            double usedTemp = 25.0;
            List<double> temperatures = GetTemperatures(c);
            if (temperatures.Count > 0)
            {
                usedTemp = temperatures.Average();
            }

            SiPMData data = new SiPMData
            {
                voltages = voltagesHandle.AddrOfPinnedObject(),
                currents = currentsHandle.AddrOfPinnedObject(),
                dataPoints = (nuint)c.IVResult.SMUCurrent.Count,
                preTemp = 25.0,
                postTemp = usedTemp,
                timestamp = (ulong)c.IVResult.StartTimestamp
            };

            Directory.CreateDirectory(outputPath);

            string outFilePrefix = $"{c.Barcode}_{c.SiPMLocation.Block}_{c.SiPMLocation.Module}_{c.SiPMLocation.Array}_{c.SiPMLocation.SiPM}";

            iv.AnalyseIV(data, AnalysisTypes.RelativeDerivativeMethod, 20.0, true, outputPath, outFilePrefix);

            c.IVResult.AnalysationResult.RootFileLocation = Path.Combine(outputPath, outFilePrefix + ".root");

            voltagesHandle.Free();
            currentsHandle.Free();

            iv.GetResult(out vbr, out cvbr, out cs);

            c.IVResult.AnalysationResult.BreakdownVoltage = vbr;
            c.IVResult.AnalysationResult.CompensatedBreakdownVoltage = cvbr;
            c.IVResult.AnalysationResult.ChiSquare = cs;

            Console.WriteLine($"Vbr: {vbr}, cVbr: {cvbr}, ChiSquare: {cs}");
            c.IVResult.AnalysationResult.Analysed = true;

            if (cs < 0.2) //fine tune this value
            {
                c.IVResult.AnalysationResult.IsOK = true;
            }
            Console.WriteLine("Analysis end");
        }

        public static int GetUsedTemperatureIndex(int arrayIndex, int sipmIndex)
        {
            int sipmIndexOffset = arrayIndex * 2 + (sipmIndex < 8 ? 0 : 1);
            return sipmIndexOffset;
        }

        public static List<double> GetTemperatures(CurrentMeasurementDataModel c)
        {
            List<double> temperatures = new List<double>();
            int index = GetUsedTemperatureIndex(c.SiPMLocation.Array, c.SiPMLocation.SiPM);
            for (int i = 0; i < c.IVResult.Temperatures.Count; i++)
            {
                if (c.SiPMLocation.Block != c.IVResult.Temperatures[i].Block)
                {
                    continue;
                }

                if (c.SiPMLocation.Module == 0)
                {
                    double temp = c.IVResult.Temperatures[i].Module1[index];
                    temperatures.Add(temp);
                }
                else
                {
                    double temp = c.IVResult.Temperatures[i].Module2[index];
                    temperatures.Add(temp);
                }
            }
            return temperatures;
        }
    }
}

