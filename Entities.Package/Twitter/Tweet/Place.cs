using System.Collections.Generic;
using Newtonsoft.Json;

namespace Entities.Twitter.Tweet
{
    public class Place
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("place_type")]
        public string PlaceType { get; set; }

        public string Country { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        public string Url { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        [JsonProperty("bounding_box")]
        public BoundingBox BoundingBox { get; set; }
    }
}