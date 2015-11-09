using Newtonsoft.Json;

namespace Entities.Twitter.RateLimiting
{
    public class RateLimitStatus
    {
        [JsonProperty("rate_limit_context")]
        public RateLimitContext RateLimitContext { get; set; }
        public RateLimitResources Resources { get; set; }
    }
}