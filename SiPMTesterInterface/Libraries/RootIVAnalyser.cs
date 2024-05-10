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
                                                    bool savePlots,
                                                    string outBasePath,
                                                    string filePrefix);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RIVA_Class_GetResults(IntPtr ivAnalyser,
                                                         out double rawVbr,
                                                         out double compVbr,
                                                         out double cs);

        /*
        public static void MyMethod()
        {
            try
            {
                TestRootFunction();
            }
            catch (DllNotFoundException ex)
            {
                // Handle the case where the library is not found
                Console.WriteLine("The required library is not available.");
                // Log or take any other appropriate action
            }
        }
        */

        private IntPtr ivAnalyser;
        private bool disposedValue;

        public RootIVAnalyser()
        {
            ivAnalyser = RIVA_Class_Create();
        }

        public void AnalyseIV(SiPMData data, AnalysisTypes type, bool savePlots, string outBasePath, string filePrefix)
        {
            RIVA_Class_AnalyseIV(ivAnalyser, data, type, savePlots, outBasePath, filePrefix);
        }

        public void GetResult(out double RawBreakdownVoltage, out double CompensatedBreakdownVoltage, out double ChiSquare)
        {
            RIVA_Class_GetResults(ivAnalyser, out RawBreakdownVoltage, out CompensatedBreakdownVoltage, out ChiSquare);
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

        public static void Analyse(CurrentMeasurementDataModel c, double DMMResistance = 0.0)
        {
            //Calculate DMM currents

            Console.WriteLine("Starting analysis...");
            RootIVAnalyser iv = new RootIVAnalyser();
            double vbr;
            double cvbr;
            double cs;

            double[] voltagesArray = c.IVResult.DMMVoltage.ToArray();
            double[] currentsArray = c.IVResult.SMUCurrent.ToArray();

            // Pin the arrays to get their pointers
            GCHandle voltagesHandle = GCHandle.Alloc(voltagesArray, GCHandleType.Pinned);
            GCHandle currentsHandle = GCHandle.Alloc(currentsArray, GCHandleType.Pinned);

            SiPMData data = new SiPMData
            {
                voltages = voltagesHandle.AddrOfPinnedObject(),
                currents = currentsHandle.AddrOfPinnedObject(),
                dataPoints = (nuint)c.IVResult.SMUCurrent.Count,
                preTemp = 25.0,
                postTemp = 30.0,
                timestamp = (ulong)c.IVResult.StartTimestamp
            };

            string outPath = Path.Combine(FilePathHelper.GetCurrentDirectory(), "results");
            Directory.CreateDirectory(outPath);

            string outFilePrefix = $"{c.SiPMLocation.Block}_{c.SiPMLocation.Module}_{c.SiPMLocation.Array}_{c.SiPMLocation.SiPM}";

            iv.AnalyseIV(data, AnalysisTypes.RelativeDerivativeMethod, true, outPath, outFilePrefix);

            voltagesHandle.Free();
            currentsHandle.Free();

            iv.GetResult(out vbr, out cvbr, out cs);

            c.IVResult.AnalysationResult.BreakdownVoltage = vbr;
            c.IVResult.AnalysationResult.CompensatedBreakdownVoltage = cvbr;
            c.IVResult.AnalysationResult.ChiSquare = cs;

            Console.WriteLine($"Vbr: {vbr}, cVbr: {cvbr}, ChiSquare: {cs}");
            c.IVResult.AnalysationResult.Analysed = true;

            if (cs < 0.08) //fine tune this value
            {
                c.IVResult.AnalysationResult.IsOK = true;
            }
            Console.WriteLine("Analysis end");
        }
    }
}

