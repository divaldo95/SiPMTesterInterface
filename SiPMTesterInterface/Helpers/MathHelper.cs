using System;
namespace SiPMTesterInterface.Helpers
{
	public static class MathHelper
	{
		public static bool IsOutOfBoundaries(this int value, int min, int max)
		{
			return !(value > min && value < max);
        }

        public static bool IsOutOfBoundaries(this double value, double min, double max)
        {
            return !(value > min && value < max);
        }

        /*
         * Returns whether value is between mean +- stdDev
         */
        public static bool IsBetweenLimits(this int value, int mean, int stdDev)
        {
            return Math.Abs(value - mean) < stdDev;
        }

        /*
         * Returns whether value is between mean +- stdDev
         */
        public static bool IsBetweenLimits(this double value, double mean, double stdDev)
        {
            return Math.Abs(value - mean) < stdDev;
        }
    }
}

