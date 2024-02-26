using System;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Classes
{
	public class MeasurementIdentifier : IEquatable<MeasurementIdentifier>
    {
        public MeasurementType Type { get; private set; }
        public int ID { get; private set; }

        public MeasurementIdentifier()
        {
            Type = MeasurementType.Unknown;
            ID = -1;
        }

        public MeasurementIdentifier(MeasurementType t, int id)
		{
            Type = t;
            ID = id;
        }

        public bool Equals(MeasurementIdentifier? other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.ID != ID || other.Type != Type)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return "Type: " + Type + ", ID: " + ID.ToString();
        }
    }
}

