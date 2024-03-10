using System;
namespace SiPMTesterInterface.Classes
{
    //Sender for specific situation
	public static class Messages
	{
		public const string IVMeasurementStart = "StartIVMeasurement";
        public const string DMMMeasurementStart = "StartDMMMeasurement";
        public const string GetMeasurementData = "GetMeasurementData";
        public const string MeasurementStartResponse = "MeasurementStart";
        public const string IVMeasurementDone = "IVMeasurementDone"; //when it is done
        public const string DMMMeasurementDone = "DMMMeasurementDone"; //when it is done
        public const string IVMeasurementQuery = "IVMeasurementResult"; //when it is queried
        public const string DMMMeasurementQuery = "DMMMeasurementResult"; //when it is queried
        public const string PingResponseSender = "Pong";
        public const string ReceiveStatusChange = "StatusChange";
        public const string GetStatus = "GetStatus";
        public const string BasicError = "Error";
    }
}

