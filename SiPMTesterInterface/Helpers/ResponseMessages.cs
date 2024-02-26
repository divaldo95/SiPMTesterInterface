using System;
using System.Text.Json;

namespace SiPMTesterInterface.Helpers
{
	public static class ResponseMessages
	{
		public static string Error(string msg)
		{
            var message = new
            {
                Error = msg
            };
            return JsonSerializer.Serialize(message);
        }
	}
}

