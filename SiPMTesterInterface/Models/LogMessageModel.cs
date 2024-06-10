using System;
using SiPMTesterInterface.Enums;
using SiPMTesterInterface.Helpers;

namespace SiPMTesterInterface.Models
{
	public class LogMessageModel
	{
        public string ID { get; set; }
        public LogMessageType MessageType { get; set; }
        public bool NeedsInteraction { get; set; } = false;
		public bool Resolved { get; set; } = false;
		public string Sender { get; set; } = "";
		public string Message { get; set; } = "";
        public ResponseButtons ValidInteractionButtons { get; set; } = ResponseButtons.OK;
		public ResponseButtons UserResponse { get; set; } = ResponseButtons.OK;
        public ErrorNextStep NextStep { get; set; } = ErrorNextStep.Undefined;
        public MeasurementType MeasurementType { get; set; } = MeasurementType.Unknown; //Where the error came from
        public long Timestamp { get; set; }

        public LogMessageModel()
		{
            Guid guid = Guid.NewGuid();
            ID = guid.ToString();
            Timestamp = TimestampHelper.GetUTCTimestamp();
        }

        public LogMessageModel(string sender, string message, LogMessageType type) : this()
        {
			Sender = sender;
			Message = message;
            MessageType = type;
        }

        public LogMessageModel(string sender, string message, LogMessageType logType, MeasurementType type) : this(sender, message, logType)
        {
            MeasurementType = type;
        }

        public LogMessageModel(string sender, string message, LogMessageType logType, bool interactionNeeded, ResponseButtons buttons) : this(sender, message, logType)
        {
			NeedsInteraction = interactionNeeded;
			ValidInteractionButtons = buttons;
        }

        public LogMessageModel(string sender, string message, LogMessageType logType, MeasurementType type, bool interactionNeeded, ResponseButtons buttons) : this(sender, message, logType, interactionNeeded, buttons)
        {
            MeasurementType = type;
        }
    }
}

