using System;
using System.Text;
using System.Net;
using System.IO;
using System.Text.Json;

namespace DNWS
{
    class PM25Plugin : IPlugin
    {
        protected static PM25Reading pm25reading = null;

        public PM25Plugin()
        {
            if (pm25reading == null)
            {
                RequestAqiCn();
            }
        }

        public void RequestAqiCn()
        {
            HttpWebRequest http = (HttpWebRequest)WebRequest.Create("http://api.waqi.info/feed/shanghai/?token=demo");
            HttpWebResponse response = (HttpWebResponse)http.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);

            string content = sr.ReadToEnd();
            pm25reading = JsonSerializer.Deserialize<PM25Reading>(content);
        }

        public void PreProcessing(HTTPRequest request)
        {
            //
        }

        public HTTPResponse GetResponse(HTTPRequest request)
        {
            // refresh
            if (pm25reading == null)
            {
                RequestAqiCn();
            }
            else
            {
                DateTime now = DateTime.Now;
                TimeSpan diff = now.Subtract(pm25reading.data.time.iso.DateTime);

                if (diff >= new TimeSpan(1, 0, 0))
                {
                    RequestAqiCn();
                }
            }

            // built JSON
            var result = new
            {
                location = pm25reading.data.city.name,
                datetime = pm25reading.data.time.iso,
                pm25 = pm25reading.data.iaqi.pm25.v,
                pm10 = pm25reading.data.iaqi.pm10.v
            };

            string json = JsonSerializer.Serialize(result);

            // response
            HTTPResponse response = new HTTPResponse(200);
            response.type = "application/json";
            response.body = Encoding.UTF8.GetBytes(json);

            return response;
        }

        public HTTPResponse PostProcessing(HTTPResponse response)
        {
            return response;
        }
    }
}