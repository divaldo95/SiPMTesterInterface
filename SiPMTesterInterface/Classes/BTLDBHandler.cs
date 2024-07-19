using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace SiPMTesterInterface.Classes
{
	public class BTLDBSettings
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public bool isHTTPS { get; set; }
		public string Endpoint { get; set; }
        public int Timeout { get; set; }

		public void CopyTo(BTLDBSettings? other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("BTLDBSettings cannot be null");
			}
			other.Host = this.Host;
			other.Port = this.Port;
			other.isHTTPS = this.isHTTPS;
			other.Endpoint = this.Endpoint;
            other.Timeout = this.Timeout;
		}
	}

    public class ChannelDto
    {
        public int ChNo { get; set; }
        public double Vop { get; set; }
        public double Id1 { get; set; }
        public double Id2 { get; set; }
        public double Is { get; set; }
        public double RqN { get; set; }
        public double TecResistance { get; set; }
    }

    public class SiPMArrayDto
    {
        public string TrayNo { get; set; }
        public int PositionNo { get; set; }
        public string SN { get; set; }
        public List<ChannelDto> Channels { get; set; }
    }

    public class BTLDBHandler
	{
		private HttpClient HttpClient;
		private BTLDBSettings settings;

		public BTLDBHandler()
		{
			HttpClient = new HttpClient();
			settings = new BTLDBSettings();
		}

		public void ApplySettings(BTLDBSettings s)
		{
			s.CopyTo(settings);
		}

		public SiPMArrayDto GetArrayDataBySN(string sn)
		{
			SiPMArrayDto data;
			//add some sn and url checks
			string url = $"http{(settings.isHTTPS ? 's': "")}://{settings.Host}:{settings.Port}/{settings.Endpoint}/{sn}/";
            try
            {
                var response = HttpClient.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    data = JsonConvert.DeserializeObject<SiPMArrayDto>(responseString);
                    Console.WriteLine($"SN: {data.SN}");
                    // Handle siPMArray as needed
                }
                else
                {
                    throw new BadHttpRequestException($"Request failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
            return data;
        }
	}
}

