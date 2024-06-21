using System;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Models
{
	public class ErrorResolveModel
	{
		public string ID { get; set; } = "";
		public ResponseButtons UserResponse { get; set; } = ResponseButtons.Cancel;
        public ErrorResolveModel()
		{
			
		}
	}
}

