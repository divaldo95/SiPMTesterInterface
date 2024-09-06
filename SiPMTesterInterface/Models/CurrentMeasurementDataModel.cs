using System;
using Newtonsoft.Json;
using SiPMTesterInterface.Classes;

namespace SiPMTesterInterface.Models
{
	public class CurrentMeasurementDataModel
	{
		public bool IsIVDone { get; set; } = false;
		public bool IsSPSDone { get; set; } = false;
		public CurrentSiPMModel SiPMLocation { get; set; }
		public string Barcode { get; set; } = "";
		public SiPM SiPMMeasurementDetails { get; set; }
		public SiPMChecks Checks { get; set; } = new SiPMChecks();
        public List<TemperaturesArray> Temperatures { get; set; } = new List<TemperaturesArray>();
		public DMMResistanceMeasurementResponseModel DMMResistanceResult { get; set; } = new DMMResistanceMeasurementResponseModel();
		public MeasurementIdentifier IVMeasurementID { get; set; } = new MeasurementIdentifier();
		public MeasurementIdentifier SPSMeasurementID { get; set; } = new MeasurementIdentifier();
        public IVMeasurementResponseModel IVResult { get; set; } = new IVMeasurementResponseModel();
		public ForwardResistanceMeasurementResponseModel ForwardResistanceResult { get; set; } = new ForwardResistanceMeasurementResponseModel();
		public DarkCurrentMeasurementResponseModel DarkCurrentResult { get; set; } = new DarkCurrentMeasurementResponseModel();

		[JsonIgnore]
		public string FileLocation { get; set; } = "";
        //placeholder for SPS response
    }
}

