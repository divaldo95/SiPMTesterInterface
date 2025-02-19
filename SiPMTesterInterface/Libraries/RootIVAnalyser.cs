using System;
using System.Runtime.InteropServices;
using SiPMTesterInterface.AnalysisModels;
using SiPMTesterInterface.Classes;
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

        public static void Analyse(CurrentMeasurementDataModel c, string outputPath, double limitVop = 0.2, AnalysisProperties? properties = null)
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
            List<double> temperatures = GetTemperatures(c, true);
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

            double hVbrc = SiPMDatasheetHandler.GetCompensatedOperatingVoltage(c.HamamatsuVbr, 20.0, usedTemp, 0);

            // Breakdown Voltage must be around Hamamatsu's Vbr
            // Compensate both to 20C and check
            if (cs < 0.2 && cvbr.IsBetweenLimits(hVbrc, limitVop) )
            {
                c.IVResult.AnalysationResult.IsOK = true;
                c.Checks.IVVbrOK = true;
            }
            Console.WriteLine("Analysis end");
        }

        public static int GetUsedTemperatureIndex(int arrayIndex, int sipmIndex)
        {
            int sipmIndexOffset = arrayIndex * 2 + (sipmIndex < 8 ? 0 : 1);
            return sipmIndexOffset;
        }

        public static List<double> GetTemperatures(CurrentMeasurementDataModel c, bool gradientCompensation = false)
        {
            List<double> temperatures = new List<double>();

            if (gradientCompensation)
            {

                double temp = 0;
                double[] averageTemp = new double[8];
                double T0, T1;  //T0 near sipm 0 T1 near sipm 15

                double[] sipmTempArray = new double[16];

                double[] InitialTemp;
                double[] FinalTemp;

                if (c.SiPMLocation.Module == 0)
                {
                    InitialTemp = c.IVResult.Temperatures.FirstOrDefault(new TemperaturesArray()).Module1;
                    FinalTemp = c.IVResult.Temperatures.LastOrDefault(new TemperaturesArray()).Module1;
                }
                else
                {
                    InitialTemp = c.IVResult.Temperatures.FirstOrDefault(new TemperaturesArray()).Module2;
                    FinalTemp = c.IVResult.Temperatures.LastOrDefault(new TemperaturesArray()).Module2;
                }

                if (InitialTemp.Length != 8)
                {
                    Console.WriteLine("Probably empty initial temperatures list");
                    InitialTemp = new double[8];
                    for (int i = 0; i < 8; i++)
                    {
                        InitialTemp[0] = 25.0;
                    }
                }

                if (FinalTemp.Length != 8)
                {
                    Console.WriteLine("Probably empty final temperatures list");
                    FinalTemp = new double[8];
                    for (int i = 0; i < 8; i++)
                    {
                        FinalTemp[0] = 25.0;
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    averageTemp[i] = (InitialTemp[i] + FinalTemp[i]) / 2;
                }

                if (c.SiPMLocation.Array == 0)
                {
                    T0 = averageTemp[0];
                    T1 = averageTemp[1];
                }
                else if (c.SiPMLocation.Array == 1)
                {
                    T0 = averageTemp[2];
                    T1 = averageTemp[3];
                }
                else if (c.SiPMLocation.Array == 2)
                {
                    T0 = averageTemp[5];
                    T1 = averageTemp[4];
                }
                else if (c.SiPMLocation.Array == 3)
                {
                    T0 = averageTemp[7];
                    T1 = averageTemp[6];
                }
                else
                {
                    throw new InvalidDataException("Array can not be larger than 3");
                }

                double delta = T0 - T1;
                double step_size = delta / 13;

                for (int i = 0; i < 14; i++)
                {
                    sipmTempArray[1 + i] = T0 - i * step_size;
                }
                sipmTempArray[15] = sipmTempArray[14] - step_size;
                sipmTempArray[0] = sipmTempArray[1] + step_size;

                temp = sipmTempArray[c.SiPMLocation.SiPM];
                temperatures.Add(temp);
            }

            else
            {
                int index = GetUsedTemperatureIndex(c.SiPMLocation.Array, c.SiPMLocation.SiPM);
                for (int i = 0; i < c.IVResult.Temperatures.Count; i++)
                {
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
            }

            return temperatures;
        }
    }
}

