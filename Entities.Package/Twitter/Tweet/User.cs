using Newtonsoft.Json;
using System;
using System.Globalization;

namespace Entities.Twitter.Tweet
{
    public class User
    {
        public long Id { get; set; }

        [JsonProperty("id_str")]
        public string IdString { get; set; }

        public string Name { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAtString { get; set; }

        public DateTime CreatedAtUtc => DateTime.ParseExact(CreatedAtString, TwitterConstants.DateFormat, new CultureInfo("en-US"));

        public DateTime CreatedAt => CreatedAtUtc.ToLocalTime();

        [JsonProperty("contributors_enabled")]
        public bool ContributorsEnabled { get; set; }

        [JsonProperty("default_profile")]
        public bool DefaultProfile { get; set; }

        [JsonProperty("default_profile_image")]
        public bool DefaultProfileImage { get; set; }

        public string Description { get; set; }

        public TweetEntities Entities { get; set; }

        [JsonProperty("favourites_count")]
        public int? FavouritesCount { get; set; }

        [JsonProperty("follow_request_sent")]
        public int? FollowRequestSent { get; set; }

        [JsonProperty("followers_count")]
        public int? FollowersCount { get; set; }

        [JsonProperty("friends_count")]
        public int? FriendsCount { get; set; }

        [JsonProperty("geo_enabled")]
        public bool GeoEnabled { get; set; }

        [JsonProperty("is_translator")]
        public bool IsTranslator { get; set; }

        [JsonProperty("lang")]
        public string Language { get; set; }

        [JsonProperty("listed_count")]
        public int? ListedCount { get; set; }

        public string Location { get; set; }

        [JsonProperty("profile_background_color")]
        public string ProfileBackgroundColor { get; set; }

        [JsonProperty("profile_background_image_url")]
        public string ProfileBackroundImageUrl { get; set; }

        [JsonProperty("profile_background_image_url_https")]
        public string ProfileBackroundImageUrlHttps { get; set; }

        // TODO
    }
}
