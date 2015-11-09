using Newtonsoft.Json;

namespace Entities.Twitter.Tweet
{
    public class CurrentUserRetweet
    {
        public long Id { get; set; }

        [JsonProperty("id_str")]
        public string IdString { get; set; }
    }
}