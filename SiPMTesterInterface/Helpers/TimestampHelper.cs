using System;
namespace SiPMTesterInterface.Helpers
{
	public static class TimestampHelper
	{
		public static long GetUTCTimestamp()
		{
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }
	}
}

