using System;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace SiPMTesterInterface.Enums
{
    //[JsonConverter(typeof(StringEnumConverter))]
    public enum MeasurementType
	{
        //[EnumMember(Value = "IVMeasurement")]
        IVMeasurement = 0,
        //[EnumMember(Value = "SPSMeasurement")]
        SPSMeasurement = 1,
        //[EnumMember(Value = "DMMMeasurement")]
        DMMResistanceMeasurement = 2,
        //[EnumMember(Value = "Unknown")]
        Unknown = 999
    }
}

