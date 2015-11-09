using Newtonsoft.Json;

namespace Entities.Twitter.RateLimiting
{
    public class RateLimitSearch
    {
        [JsonProperty("/search/tweets")]
        public RateLimit SearchTweets { get; set; }
    }
}