using System;
using System.Collections.Generic;
using Entities.Twitter.Tweet;

namespace Entities.Twitter.SearchIndex
{
    public class FlattendTweet
    {
        public string TweetId { get; set; }
        public string Username { get; set; }
        public string TweetMessage { get; set; }
        public string Date { get; set; }
        public int? RetweetCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string WeatherDescription { get; set; }
        public double? Temperature { get; set; }
        public double? Cloudiness { get; set; }
        public double? Humidity { get; set; }
        public string IconUrl { get; set; }
        public Coordinate TweetCoordinates { get; set; }
        public List<string> HashTags { get; set; }
        public string WeatherId { get; set; }
        public double? WindSpeed { get; set; }
        public double? WindDegree { get; set; }
        public string WeatherTitle { get; set; }
        public List<string> Urls { get; set; }

        public override string ToString()
        {
            return $"ID: {TweetId}\tUser: {Username}\tDate{Date}\nTweet:{TweetMessage}";
        }
    }
}