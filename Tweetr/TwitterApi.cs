using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Entities.Twitter.RateLimiting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Entities.Twitter.Api;

namespace Tweetr
{
    class TwitterApi
    {
        private const string BaseUrl = "https://api.twitter.com";

        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private string _bearerToken;

        public TwitterApi(string consumerKey, string consumerSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
        }

        public RateLimitStatus GetRateLimits(params string[] resources)
        {
            var queryString = string.Empty;

            if (resources.Any())
            {
                queryString = "resources=" + string.Join(",", resources);
            }

            var request = CreateGetRequestWithAuthorization("/1.1/application/rate_limit_status.json?" + queryString);
            var response = ReadResponse(request);

            return JsonConvert.DeserializeObject<RateLimitStatus>(response);
        }

        public SearchTweetsResult SearchTweets(SearchTweetsRequest request) => SearchTweetsInternal(request.GetQueryString());

        private SearchTweetsResult SearchTweetsInternal(string queryString)
        {
            if (string.IsNullOrWhiteSpace(queryString))
                throw new ArgumentException("Query string is empty", nameof(queryString));

            var request = CreateGetRequestWithAuthorization("/1.1/search/tweets.json?" + queryString);
            var response = ReadResponse(request);

            return JsonConvert.DeserializeObject<SearchTweetsResult>(response);
        }

        private string GetBearerToken()
        {
            if (_bearerToken == null)
            {
                var request = CreateRequest("/oauth2/token");
                request.Headers.Add("Authorization", "Basic " + GetConsumerToken());

                WritePostRequest(request, "grant_type=client_credentials");

                var response = ReadResponse(request);
                var jsonObject = JObject.Parse(response);

                _bearerToken = jsonObject.SelectToken("access_token").Value<string>();
            }

            return _bearerToken;
        }

        private string GetConsumerToken()
        {
            var encodedConsumerKey = Uri.EscapeDataString(_consumerKey);
            var encodedConsumerSecret = Uri.EscapeDataString(_consumerSecret);

            var consumerToken = Base64Encode(
                string.Format("{0}:{1}", encodedConsumerKey, encodedConsumerSecret));

            return consumerToken;
        }

        private WebRequest CreateGetRequestWithAuthorization(string relativeUrl)
        {
            var request = CreateRequest(relativeUrl);
            request.Method = WebRequestMethods.Http.Get;
            request.Headers.Add("Authorization", "Bearer " + GetBearerToken());

            return request;
        }

        private static WebRequest CreateRequest(string relativeUrl)
        {
            var request = WebRequest.Create(BaseUrl + relativeUrl);
            ((HttpWebRequest)request).UserAgent = "GeekCruise";

            return request;
        }

        private static void WritePostRequest(WebRequest request, string postData)
        {
            var dataBytes = Encoding.UTF8.GetBytes(postData);

            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            request.ContentLength = dataBytes.Length;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(dataBytes, 0, dataBytes.Length);
            }
        }

        private static string ReadResponse(WebRequest request)
        {
            var responseStream = request.GetResponse().GetResponseStream();
            if (responseStream == null)
                return null;

            using (var responseReader = new StreamReader(responseStream))
            {
                return responseReader.ReadToEnd();
            }
        }

        private static string Base64Encode(string str)
        {
            var bytes = Encoding.ASCII.GetBytes(str);
            return Convert.ToBase64String(bytes);
        }
    }
}
