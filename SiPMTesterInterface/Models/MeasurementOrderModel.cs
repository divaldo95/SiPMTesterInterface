using System;
using System.Text;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Models
{
	public class MeasurementOrderModel
	{
		public TaskTypes Task { get; set; }
		public MeasurementType Type { get; set; }
		public CurrentSiPMModel SiPM { get; set; }
		public object? StartModel { get; set; }

        public override string ToString()
        {
			StringBuilder sb = new StringBuilder();
			sb.Append($"Task: {Task} | Measurement Type: {Type} | SiPM: {SiPM.Block}, {SiPM.Module}, {SiPM.Array}, {SiPM.SiPM}");
			if (Type == MeasurementType.DarkCurrentMeasurement)
			{
				var startModel = StartModel as NIVoltageAndCurrentStartModel;
                sb.Append($" | {startModel.MeasurementType}");
            }
            
            return sb.ToString();
        }
    }
}

