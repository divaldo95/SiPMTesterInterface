using System;
namespace SiPMTesterInterface.Enums
{
	/* 
	 * An enum which holds what can be the next step after resolving an error.
	 */
	
	public enum ErrorNextStep
	{
		Continue = 0, //skip last sipm
		Stop = 1, //stop measurement
		Retry = 2, //try last sipm
		ReInitNI = 3, //reinit NI connection
		ReInitPulser = 4, //reinit serial pulser
		ReInitHVPSU = 5, //reinit serial hvpsu
		ReInitDAQ = 6, //reinit DAQ device
		Undefined = 7 //let the user decide
	}
}

