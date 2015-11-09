using Newtonsoft.Json;

namespace Entities.Twitter.Tweet
{
    public class TweetUrl
    {
        public string Url { get; set; }

        [JsonProperty("display_url")]
        public string DisplayUrl { get; set; }

        [JsonProperty("expanded_url")]
        public string ExpandedUrl { get; set; }

        public int[] Indices { get; set; }
    }
}