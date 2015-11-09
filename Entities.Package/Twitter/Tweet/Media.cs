using Newtonsoft.Json;

namespace Entities.Twitter.Tweet
{
    public class Media
    {
        public long Id { get; set; }

        [JsonProperty("id_str")]
        public string IdString { get; set; }

        public string Type { get; set; }

        public int[] Indices { get; set; }

        public string Url { get; set; }

        [JsonProperty("display_url")]
        public string DisplayUrl { get; set; }

        [JsonProperty("expanded_url")]
        public string ExpandedUrl { get; set; }

        [JsonProperty("media_url")]
        public string MediaUrl { get; set; }

        [JsonProperty("media_url_https")]
        public string MediaUrlHttps { get; set; }

        public MediaSizes Sizes { get; set; }

        [JsonProperty("source_status_id")]
        public long? SourceStatusId { get; set; }

        [JsonProperty("source_status_id_str")]
        public string SourceStatusIdString { get; set; }
    }
}