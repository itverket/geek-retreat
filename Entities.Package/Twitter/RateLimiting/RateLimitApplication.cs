using Newtonsoft.Json;

namespace Entities.Twitter.RateLimiting
{
    public class RateLimitApplication
    {
        [JsonProperty("/application/rate_limit_status")]
        public RateLimit ApplicationRateLimitStatus { get; set; }
    }
}