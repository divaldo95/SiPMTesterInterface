using System;
using System.Text;

namespace SiPMTesterInterface.Models
{
	public class CurrentSiPMModel
    {
        public int Block { get; set; }
        public int Module { get; set; }
        public int Array { get; set; }
        public int SiPM { get; set; }

		public CurrentSiPMModel()
		{
            Block = -1;
            Module = -1;
            Array = -1;
			SiPM = -1;
		}

        public CurrentSiPMModel(int block, int module, int array, int sipm)
        {
            Block = block;
            Module = module;
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
            Block = iArray[0];
            Module = iArray[1];
            Array = iArray[2];
            SiPM = iArray[3];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Block: {Block}\t|\t");
            sb.Append($"Module: {Module}\t|\t");
            sb.Append($"Array: {Array}\t|\t");
            sb.Append($"SiPM: {SiPM}");
            return sb.ToString();
        }
    }
}

