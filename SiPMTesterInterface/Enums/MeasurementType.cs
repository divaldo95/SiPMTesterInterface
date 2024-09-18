using System;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace SiPMTesterInterface.Enums
{
    public enum MeasurementType
	{
        IVMeasurement = 0,
        SPSMeasurement = 1,
        DMMResistanceMeasurement = 2,
        DarkCurrentMeasurement = 3,
        ForwardResistanceMeasurement = 4,
        TemperatureMeasurement = 5,
        Analysis = 6,
        Unknown = 999
    }
}

