using System;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.Models;

namespace SiPMTesterInterface.Helpers
{
	public static class TemperatureSelectHelper
	{
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
                if (c.SiPMLocation.Module == 0)
                {
                    double temp = c.IVResult.Temperatures[i].Module1[index];
                    if (temp < -60)
                    {
                        Console.WriteLine($"Temperature exluded because it is too low ({temp})");
                    }
                    else
                    {
                        temperatures.Add(temp);
                    }
                    
                }
                else
                {
                    double temp = c.IVResult.Temperatures[i].Module2[index];
                    if (temp < -60)
                    {
                        Console.WriteLine($"Temperature exluded because it is too low ({temp})");
                    }
                    else
                    {
                        temperatures.Add(temp);
                    }
                }
            }
            return temperatures;
        }

        public static List<double> GetTemperatures(List<TemperaturesArray> temperaturesArray, int Block, int Module, int Array, int SiPM, long timestamp = 0)
        {
            bool foundAfterTimestamp = true;
            List<double> temperatures = new List<double>();
            int index = GetUsedTemperatureIndex(Array, SiPM);

            var t1 = temperaturesArray.Where(temp => temp.Timestamp >= timestamp && temp.Block == Block).ToList();
            if (t1.Count == 0)
            {
                t1 = temperaturesArray.Where(temp => temp.Block == Block).ToList();
                foundAfterTimestamp = false;
            }

            //if found, take the average of these values, timestamp makes sure to not include temperatures before measurement starts
            //else get the last known value
            if (foundAfterTimestamp) 
            {
                for (int i = 0; i < t1.Count; i++)
                {
                    if (t1[i].Block != Block || t1[i].Timestamp < timestamp)
                    {
                        continue;
                    }
                    if (Module == 0)
                    {
                        double temp = t1[i].Module1[index];
                        temperatures.Add(temp);
                    }
                    else
                    {
                        double temp = t1[i].Module2[index];
                        temperatures.Add(temp);
                    }
                }
            }
            else
            {
                try
                {
                    TemperaturesArray t = t1.Last();
                    if (Module == 0)
                    {
                        double temp = t.Module1[index];
                        temperatures.Add(temp);
                    }
                    else
                    {
                        double temp = t.Module2[index];
                        temperatures.Add(temp);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Temperature measurement not found: {ex.Message}");
                    Console.WriteLine($"Temperature measurement not found: {ex.StackTrace}");
                }
            }
            
            return temperatures;
        }
    }
}

