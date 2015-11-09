using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Globalization;

namespace Entities.Twitter.Tweet
{
    public class Tweet
    {
        public long Id { get; set; }

        [JsonProperty("id_str")]
        public string IdString { get; set; }

        public List<Contributor> Contributors { get; set; }

        public Coordinate Coordinates { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAtString { get; set; }

        public DateTime CreatedAtUtc => DateTime.ParseExact(CreatedAtString, TwitterConstants.DateFormat, new CultureInfo("en-US"));

        public DateTime CreatedAt => CreatedAtUtc.ToLocalTime();

        [JsonProperty("current_user_retweet")]
        public CurrentUserRetweet CurrentUserRetweet { get; set; }

        public TweetEntities Entities { get; set; }

        [JsonProperty("favorite_count")]
        public int? FavoriteCount { get; set; }

        public bool? Favorited { get; set; }

        [JsonProperty("filter_level")]
        public string FilterLevel { get; set; }

        [JsonProperty("in_reply_to_screen_name")]
        public string InReplyToScreenName { get; set; }

        [JsonProperty("in_reply_to_status_id")]
        public long? InReplyToStatusId { get; set; }

        [JsonProperty("in_reply_to_status_id_str")]
        public string InReplyToStatusIdString { get; set; }

        [JsonProperty("in_reply_to_user_id")]
        public long? InReplyToUserId { get; set; }

        [JsonProperty("in_reply_to_user_id_str")]
        public string InReplyToUserIdString { get; set; }

        [JsonProperty("lang")]
        public string Language { get; set; }

        public Place Place { get; set; }

        [JsonProperty("possibly_sensitive")]
        public bool? PossiblySensitive { get; set; }

        [JsonProperty("retweet_count")]
        public int? RetweetCount { get; set; }

        public bool Retweeted { get; set; }

        [JsonProperty("retweeted_status")]
        public Tweet RetweetedStatus { get; set; }

        public string Source { get; set; }

        public string Text { get; set; }

        public bool Truncated { get; set; }

        public User User { get; set; }

        [JsonProperty("withheld_copyright")]
        public bool WithheldCopyright { get; set; }

        [JsonProperty("withheld_in_countries")]
        public List<string> WithheldInCountries { get; set; }

        [JsonProperty("withheld_scope")]
        public string WithheldScope { get; set; }
    }
}