using System.Diagnostics;
using System.Net;
using Newtonsoft.Json.Linq;

namespace WeatherUndergroundService.Util.Api
{
    public static class HistoryApiClient
    {
        public static JObject GetResponse(string queryString)
        {
            using (var client = new WebClient())
            {
                var apiKey = ClientSettings.ApiKey;
                var apiUrl = ClientSettings.HistoryApiUrl;
                Trace.WriteLine("<HTTP - GET - " + queryString + " >");
                string url;
                if (!string.IsNullOrEmpty(apiKey))
                    url = apiUrl + queryString + "&APPID=" + apiKey;
                else
                    url = apiUrl + queryString;

                var response = client.DownloadString(url);
                var parsedResponse = JObject.Parse(response);
                return parsedResponse;
            }
        }
    }
}