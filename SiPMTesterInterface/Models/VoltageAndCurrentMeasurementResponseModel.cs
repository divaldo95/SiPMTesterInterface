using System;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
	public class VoltageAndCurrentMeasurementResponseModel
	{
        public MeasurementIdentifier Identifier { get; set; }
        public NIVoltageAndCurrentStartModel StartModel { get; set; }
        public long StartTimestamp { get; set; }
        public long EndTimestamp { get; set; }
        public bool ErrorHappened { get; set; }
        public string ErrorMessage { get; set; }
        public List<double> FirstIterationVoltages { get; set; }
        public List<double> FirstIterationCurrents { get; set; }
        public List<double> SecondIterationVoltages { get; set; }
        public List<double> SecondIterationCurrents { get; set; }
        public double FirstIterationVoltageAverage { get; set; }
        public double SecondIterationVoltageAverage { get; set; }
        public double FirstIterationCurrentAverage { get; set; }
        public double SecondIterationCurrentAverage { get; set; }
        public List<TemperaturesArray> Temperatures { get; set; }
    }
}

