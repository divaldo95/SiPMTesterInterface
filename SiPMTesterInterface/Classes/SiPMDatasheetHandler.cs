using System;
using Newtonsoft.Json.Linq;

namespace SiPMTesterInterface.Classes
{
	//placeholder to get the actual Vop for a given SiPM
	public class SiPMDatasheetHandler
	{
		//placeholder function, replace with db implementation
		public static double GetSiPMVop(string barcode, int SiPM)
		{
			if (string.IsNullOrEmpty(barcode) || string.IsNullOrWhiteSpace(barcode))
			{
				throw new ArgumentException("Invalid barcode");
			}
			if (SiPM < 0 || SiPM > 15)
			{
				throw new ArgumentException("Invalid SiPM number");
			}

			return 38.0;
		}

		public static double GetCompensatedOperatingVoltage(double operatingVoltage, double temperatureTo, double temperatureAt = 20.0, double datasheetOffset = 3.0)
		{
			double deltaTemperature = temperatureTo - temperatureAt;
			double Vop = (operatingVoltage - datasheetOffset) + (0.037 * deltaTemperature);
            Console.WriteLine($"Raw Vop: {operatingVoltage}@{temperatureAt}C | Compensated Vop: {Vop}@{temperatureTo}C ");
            return Vop;
        }

		public static List<double> GenerateIVVoltageList(double compensatedOperatingVoltage)
		{
            List<double> ivList = new List<double>();

            double start = compensatedOperatingVoltage - 0.5;
            double end = compensatedOperatingVoltage + 0.5;
            double step = 0.02;

            for (double i = start; i < end; i = i + step)
            {
                ivList.Add(Math.Round(i, 2, MidpointRounding.AwayFromZero));
            }

            return ivList;
        }

		public static List<double> GenerateIVVoltageList(double operatingVoltage, double temperatureTo, double temperatureAt = 20.0, double datasheetOffset = 3.0)
		{
			double Vop = GetCompensatedOperatingVoltage(operatingVoltage, temperatureTo, temperatureAt, datasheetOffset);
			return GenerateIVVoltageList(Vop, datasheetOffset);

        }

		public SiPMDatasheetHandler()
		{
		}
	}
}

