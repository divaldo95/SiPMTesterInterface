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
        public Devices Device { get; set; } = Devices.Unknown;
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

        public LogMessageModel(string sender, string message, Devices device, LogMessageType type) : this()
        {
			Sender = sender;
			Message = message;
            MessageType = type;
            Device = device;
        }

        public LogMessageModel(string sender, string message, LogMessageType logType, Devices device, MeasurementType type) : this(sender, message, device, logType)
        {
            MeasurementType = type;
        }

        public LogMessageModel(string sender, string message, LogMessageType logType, Devices device, bool interactionNeeded, ResponseButtons buttons) : this(sender, message, device, logType)
        {
			NeedsInteraction = interactionNeeded;
			ValidInteractionButtons = buttons;
        }

        public LogMessageModel(string sender, string message, LogMessageType logType, Devices device, MeasurementType type, bool interactionNeeded, ResponseButtons buttons) : this(sender, message, logType, device, interactionNeeded, buttons)
        {
            MeasurementType = type;
        }
    }
}

