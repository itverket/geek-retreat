using System.Collections.Generic;
using Newtonsoft.Json;

namespace Entities.Twitter.Tweet
{
    public class TweetEntities
    {
        public List<Hashtag> Hashtags { get; set; }

        public List<Media> Media { get; set; }

        public List<TweetUrl> Urls { get; set; }

        [JsonProperty("user_mentions")]
        public List<UserMention> UserMentions { get; set; }
    }
}