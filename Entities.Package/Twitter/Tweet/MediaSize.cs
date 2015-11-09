using Newtonsoft.Json;

namespace Entities.Twitter.Tweet
{
    public class MediaSize
    {
        [JsonProperty("w")]
        public int Width { get; set; }

        [JsonProperty("h")]
        public int Height { get; set; }

        public string Resize { get; set; }
    }
}