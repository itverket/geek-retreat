using Newtonsoft.Json;

namespace Entities.Twitter.Tweet
{
    public class Contributor
    {
        public long Id { get; set; }

        [JsonProperty("id_str")]
        public string IdString { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }
    }
}