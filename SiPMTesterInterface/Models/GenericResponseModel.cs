using System;
using Newtonsoft.Json.Linq;

namespace SiPMTesterInterface.Models
{
	public class GenericResponseModel
	{
        public string Sender { get; set; } = "";
        public JObject jsonObject { get; set; } = new JObject();

        public GenericResponseModel()
        {

        }

        public GenericResponseModel(string s, JObject obj)
        {
            Sender = s;
            jsonObject = obj;
        }
    }
}

