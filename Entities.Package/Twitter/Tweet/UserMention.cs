using Newtonsoft.Json;

namespace Entities.Twitter.Tweet
{
    public class UserMention
    {
        public long Id { get; set; }

        [JsonProperty("id_str")]
        public string IdString { get; set; }

        public int[] Indices { get; set; }

        public string Name { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }
    }
}