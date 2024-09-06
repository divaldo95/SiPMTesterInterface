using System;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SiPMTesterInterface.Models
{
	public class ForwardResistanceMeasurementResponseModel : IEquatable<MeasurementIdentifier>
	{
        public double InstrumentResistance { get; set; } = 0.0;
		public VoltageAndCurrentMeasurementResponseModel Result { get; set; } = new VoltageAndCurrentMeasurementResponseModel();

        public double ForwardResistance
        {
            get
            {
                if (Result == null)
                {
                    throw new NullReferenceException("Forward Resistance Result is null");
                }
                return (Math.Abs(Result.SecondIterationVoltageAverage) - Math.Abs(Result.FirstIterationVoltageAverage)) / (Math.Abs(Result.SecondIterationCurrentAverage) - Math.Abs(Result.FirstIterationCurrentAverage)) - 49.9 - InstrumentResistance;
            }
        }

        public ForwardResistanceMeasurementResponseModel()
		{
		}

        //Get by identifier
        public bool Equals(MeasurementIdentifier? other)
        {
            if (other == null)
            {
                return false;
            }

            if (Result != null && Result.Identifier != null)
            {
                if (Result.Identifier.Equals(other))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

