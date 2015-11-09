using System.Collections.Generic;
using Newtonsoft.Json;

namespace Entities.Twitter.Api
{
    public class SearchTweetsResult
    {
        [JsonProperty("statuses")]
        public List<Tweet.Tweet> Tweets { get; set; }
    }
}
