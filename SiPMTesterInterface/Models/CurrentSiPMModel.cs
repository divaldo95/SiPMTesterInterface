using System;
namespace SiPMTesterInterface.Models
{
	public class CurrentSiPMModel
	{
        public int Array { get; set; }
        public int SiPM { get; set; }

		public CurrentSiPMModel()
		{
			Array = -1;
			SiPM = -1;
		}

        public CurrentSiPMModel(int array, int sipm)
        {
            Array = array;
            SiPM = sipm;
        }

        public CurrentSiPMModel(string nimachineResponse)
        {
            string[] sArray = nimachineResponse.Split(',');
            int[] iArray = new int[sArray.Length];
            if (sArray.Length != 2)
            {
                throw new ArgumentException("Cannot parse NIMachine response");
            }
            for (int i = 0; i < sArray.Length; i++)
            {
                iArray[i] = int.Parse(sArray[i].Replace(',', '.'));
            }
            Array = iArray[0];
            SiPM = iArray[1];
        }
    }
}

