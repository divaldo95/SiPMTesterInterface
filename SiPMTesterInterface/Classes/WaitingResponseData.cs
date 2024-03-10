using System;
using System.Threading;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Classes
{
	public class WaitingResponseData : IEquatable<WaitingResponseData>
	{
        public MeasurementIdentifier Identifier;
		public Timer _timer { get; set; }
        public int Retries { get; private set; }

        private Action<WaitingResponseData> CallbackFunction;
        private Action<WaitingResponseData> FailCallbackFunction; //if fails for more than 3 times mark as failed

        private void TimerCallback(object? state)
        {
            //Try 3 times and handle if fails
            if (Retries > 2)
            {
                FailCallbackFunction(this);
            }

            CallbackFunction(this);
            Retries++;

        }

        public void ChangeTimerInterval(TimeSpan period)
        {
            _timer.Change(period, period); //wait the period for the first time too
        }

        public void ChangeTimerInterval(int timeout)
        {
            _timer.Change(timeout, timeout); //wait the period for the first time too
        }

        public void Start(TimeSpan period)
        {
            ChangeTimerInterval(period);
        }

        public void Stop()
        {
            ChangeTimerInterval(Timeout.Infinite);
        }

        public void Close()
        {
            _timer.Dispose();
        }

        public bool Equals(WaitingResponseData? other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.Identifier != this.Identifier)
            {
                return false;
            }
            return true;
        }

        public bool Equals(MeasurementIdentifier? other)
        {
            if (other == null)
            {
                return false;
            }

            if (other != this.Identifier)
            {
                return false;
            }
            return true;
        }

        public WaitingResponseData(MeasurementIdentifier i, Action<WaitingResponseData> callback, Action<WaitingResponseData> failCallback)
        {
            Identifier = i;
            CallbackFunction = callback;
            FailCallbackFunction = failCallback;
            _timer = new Timer(TimerCallback, this, Timeout.Infinite, Timeout.Infinite);
        }

        public WaitingResponseData(MeasurementType t, string id, Action<WaitingResponseData> callback, Action<WaitingResponseData> failCallback)
		{
            Identifier = new MeasurementIdentifier(t, id);
            CallbackFunction = callback;
            FailCallbackFunction = failCallback;
            _timer = new Timer(TimerCallback, this, Timeout.Infinite, Timeout.Infinite);
        }
	}
}

