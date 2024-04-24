using System;
using System.Runtime.InteropServices;
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
        public static extern void RIVA_Class_AnalyseIV(IntPtr ivAnalyser,
                                                   double[] voltages,
                                                   double[] currents,
                                                   UIntPtr dataPoints,
                                                   double preTemp,
                                                   double postTemp,
                                                   int arrayID,
                                                   int sipmID,
                                                   ulong timestamp,
                                                   string outBasePath);

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

        public void AnalyseIV(double[] voltages, double[] currents, UIntPtr dataPoints, double preTemp, double postTemp,
                                int arrayID, int sipmID, ulong timestamp, string outBasePath)
        {
            RIVA_Class_AnalyseIV(ivAnalyser, voltages, currents, dataPoints, preTemp, postTemp, arrayID, sipmID, timestamp, outBasePath);
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

        public static void TestLibrary()
        {
            try
            {
                Console.WriteLine("Starting analysis...");
                RootIVAnalyser iv = new RootIVAnalyser();
                double vbr;
                double cvbr;
                double cs;
                iv.GetResult(out vbr, out cvbr, out cs);

                Console.WriteLine($"Vbr: {vbr.ToString()}, cVbr: {cvbr.ToString()}, ChiSquare: {cs}");

                CurrentMeasurementDataModel c = JSONHelper.ReadJsonFile<CurrentMeasurementDataModel>(FilePathHelper.GetCurrentDirectory() + "IV_0_0_0_0.json");

                iv.AnalyseIV(c.IVResult.DMMVoltage.ToArray(), c.IVResult.SMUCurrent.ToArray(), (nuint)c.IVResult.SMUCurrent.Count, 25.0, 26.0, 0, 0, (ulong)c.IVResult.StartTimestamp, FilePathHelper.GetCurrentDirectory());


                iv.GetResult(out vbr, out cvbr, out cs);

                Console.WriteLine($"Vbr: {vbr.ToString()}, cVbr: {cvbr.ToString()}, ChiSquare: {cs}");

                Console.WriteLine("Analysis end");
            }
            catch (DllNotFoundException ex)
            {
                // Handle the case where the library is not found
                Console.WriteLine($"The required library is not available: {ex.Message}");
                // Log or take any other appropriate action
            }
            
        }
    }
}

