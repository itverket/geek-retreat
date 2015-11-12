using System;
using System.Collections.Generic;
using System.Linq;
using Entities.Twitter.SearchIndex;
using Entities.Twitter.Tweet;
using WeatherUndergroundService.Clients;
using Coordinate = Entities.Twitter.SearchIndex.Coordinate;


namespace SearchIndexWorker
{
    public class TweetsEnricher
    {
        public List<FlattendTweet> FlattenAndEnrichTweets(List<Tweet> tweets)
        {
            var flattendedTweets = tweets.Select(tweet => new FlattendTweet
            {
                TweetId = tweet.IdString,
                CreatedAt = tweet.CreatedAtUtc,
                Date = tweet.CreatedAt.ToShortDateString(),
                Username = tweet.User.ScreenName,
                TweetMessage = tweet.Text,
                TweetCoordinates = new Coordinate{ coordinates = tweet.Coordinates.Coordinates, type = tweet.Coordinates.Type},
                RetweetCount = tweet.RetweetCount ?? 0,
                HashTags = tweet.Entities.Hashtags.Select(h => h.Text).ToList(),
                Urls = tweet.Entities.Urls.Select(u => u.ExpandedUrl).ToList()
            }).ToList();

            GetWeatherForTweets(flattendedTweets);

            return flattendedTweets;
        }
        private static void GetWeatherForTweets(IEnumerable<FlattendTweet> flattendedTweets)
        {
            foreach (var tweet in flattendedTweets)
            {
                if (tweet.TweetCoordinates == null || !tweet.CreatedAt.HasValue || DateTime.Now.AddMonths(-1) > tweet.CreatedAt)
                {
                    SetFilterableTemperature(tweet);
                    continue;
                }
                var historicResult = HistoricWeather.GetByCoordinates((double)tweet.TweetCoordinates.coordinates[0], (double)tweet.TweetCoordinates.coordinates[1], tweet.CreatedAt.Value.Add(new TimeSpan(0, -6, 0)), tweet.CreatedAt.Value.Add(new TimeSpan(0, 6, 0)));
                var bestWeather = historicResult.OrderBy(weather => Math.Abs((weather.Date - tweet.CreatedAt.Value).Ticks)).FirstOrDefault();
                if (bestWeather == null)
                {
                    SetFilterableTemperature(tweet);
                    continue;
                }

                tweet.WeatherId = bestWeather.WeatherId;
                tweet.WeatherDescription = bestWeather.Description;
                tweet.WeatherTitle = bestWeather.Title;
                tweet.Temperature = bestWeather.Temp;
                tweet.Cloudiness = bestWeather.Clouds;
                tweet.Humidity = bestWeather.Humidity;
                tweet.WindSpeed = bestWeather.WindSpeed;
                tweet.WindDegree = bestWeather.WindDegree;
                tweet.IconUrl = bestWeather.Icon;
            }
        }

        private static  void SetFilterableTemperature(FlattendTweet tweet)
        {   // set a temperature we can filter out, to enable faceting on weather
            tweet.Temperature = -100.0;
        }
    }
}