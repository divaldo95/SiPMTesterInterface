using System;
using Newtonsoft.Json;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
	public class IVMeasurementResponseModel
	{
		public MeasurementIdentifier Identifier { get; set; }
		public long StartTimestamp { get; set; }
		public long EndTimestamp { get; set; }
		public bool ErrorHappened { get; set; }
		public string ErrorMessage { get; set; }
		public List<TemperaturesArray> Temperatures { get; set; }
        public List<double> SMUVoltage { get; set; }
        public List<double> SMUCurrent { get; set; }
        public List<double> DMMVoltage { get; set; }
		public IVAnalysationResult AnalysationResult { get; set; } = new IVAnalysationResult();
    }
}

