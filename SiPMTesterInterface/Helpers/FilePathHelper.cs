using System;
namespace SiPMTesterInterface.Helpers
{
	public static class FilePathHelper
	{
		public static string GetCurrentDirectory()
		{
            return AppDomain.CurrentDomain.BaseDirectory;
        }
	}
}

