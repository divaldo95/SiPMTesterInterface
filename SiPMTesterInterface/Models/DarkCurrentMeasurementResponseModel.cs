using System;
using SiPMTesterInterface.Classes;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Models
{
	public class DarkCurrentMeasurementResponseModel : IEquatable<MeasurementIdentifier>
	{
		public VoltageAndCurrentMeasurementResponseModel LeakageCurrentResult { get; set; } = new VoltageAndCurrentMeasurementResponseModel();
        public VoltageAndCurrentMeasurementResponseModel DarkCurrentResult { get; set; } = new VoltageAndCurrentMeasurementResponseModel();

        public double FirstLeakageCurrent
        {
            get
            {
                if (LeakageCurrentResult == null)
                {
                    throw new NullReferenceException("LeakageCurrentResult is null");
                }
                return LeakageCurrentResult.FirstIterationCurrentAverage;
            }
        }

        public double SecondLeakageCurrent
        {
            get
            {
                if (LeakageCurrentResult == null)
                {
                    throw new NullReferenceException("LeakageCurrentResult is null");
                }
                return LeakageCurrentResult.SecondIterationCurrentAverage;
            }
        }

        public double FirstDarkCurrent
        {
            get
            {
                if (DarkCurrentResult == null)
                {
                    throw new NullReferenceException("DarkCurrentResult is null");
                }
                return DarkCurrentResult.FirstIterationCurrentAverage;
            }
        }

        public double SecondDarkCurrent
        {
            get
            {
                if (DarkCurrentResult == null)
                {
                    throw new NullReferenceException("DarkCurrentResult is null");
                }
                return DarkCurrentResult.SecondIterationCurrentAverage;
            }
        }

        public double FirstDarkCurrentCompensated
        {
            get
            {
                if (DarkCurrentResult == null || LeakageCurrentResult == null)
                {
                    throw new NullReferenceException("DarkCurrentResult or LeakageCurrentResult is null");
                }
                return DarkCurrentResult.FirstIterationCurrentAverage - LeakageCurrentResult.FirstIterationCurrentAverage;
            }
        }

        public double SecondDarkCurrentCompensated
        {
            get
            {
                if (DarkCurrentResult == null || LeakageCurrentResult == null)
                {
                    throw new NullReferenceException("DarkCurrentResult or LeakageCurrentResult is null");
                }
                return DarkCurrentResult.SecondIterationCurrentAverage - LeakageCurrentResult.FirstIterationCurrentAverage;
            }
        }

        public DarkCurrentMeasurementResponseModel()
		{
		}

        //To get the right measurement by only one matching identifier
        public bool Equals(MeasurementIdentifier? other)
        {
            if (other == null)
            {
                return false;
            }

            if (LeakageCurrentResult != null && LeakageCurrentResult.Identifier != null)
            {
                if (LeakageCurrentResult.Identifier.Equals(other))
                {
                    return true;
                }
            }
            if (DarkCurrentResult != null && DarkCurrentResult.Identifier != null)
            {
                if (DarkCurrentResult.Identifier.Equals(other))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

