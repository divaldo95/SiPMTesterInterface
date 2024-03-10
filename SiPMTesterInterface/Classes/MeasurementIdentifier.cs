using System;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Classes
{
	public class MeasurementIdentifier : IEquatable<MeasurementIdentifier>
    {
        public MeasurementType Type { get; set; }
        public string ID { get; set; }

        public MeasurementIdentifier()
        {
            Type = MeasurementType.Unknown;
            ID = "";
            GenerateID();
        }

        public MeasurementIdentifier(MeasurementType type)
        {
            Type = type;
            ID = "";
            GenerateID();
        }

        public MeasurementIdentifier(MeasurementType t, string id)
		{
            Type = t;
            ID = id;
        }

        public void GenerateID()
        {
            Guid guid = Guid.NewGuid();
            ID = guid.ToString();
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

