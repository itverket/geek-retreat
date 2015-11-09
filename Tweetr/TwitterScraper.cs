using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Entities.Twitter.Api;
using Entities.Twitter.RateLimiting;
using Entities.Twitter.Tweet;
using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Storage.Azure.Table;
using Storage.Azure.Table.Entities.Twitter;

namespace Tweetr
{


    class TwitterScraper
    {
        private const long InitialTweetId = 595012389977903104L;

        private readonly TwitterApi _twitterApi;
        private readonly AzureTableStorage _tableStorage;

        private long _lastInsertedTweetId;
        private AzureTableStorage _secondTableStorage;
        //private TopicClient _topicClient;

        public TwitterScraper(TwitterApi twitterApi, AzureTableStorage tableStorage, AzureTableStorage tabelStorage2)
        {
            _twitterApi = twitterApi;
            _tableStorage = tableStorage;
            _secondTableStorage = tabelStorage2;
            //_topicClient = topicClient;
        }

        public Task RunAsync(CancellationToken cancellationToken) => Task.Factory.StartNew(() => Scrape(cancellationToken));

        private void Scrape(CancellationToken cancellationToken)
        {
            Trace.TraceInformation("Twitter scraper started");

            InitializeLastTweetId();

            var searchRateLimit = GetSearchRateLimit();

            Trace.TraceInformation($"Remaining requests: { searchRateLimit.Remaining }, Reset: { searchRateLimit.ResetDateTime.ToShortTimeString() }");

            if (searchRateLimit.Remaining == 0)
            {
                Trace.TraceInformation($"Rate limit reached. Sleeping until limit is reset. ({ searchRateLimit.MillisecondsToReset() } ms)");

                Thread.Sleep(searchRateLimit.MillisecondsToReset() + 3000);

                return;
            }

            var minMillisBetweenRequests = searchRateLimit.MillisecondsToReset() / searchRateLimit.Remaining;

            var requestCount = 0;
            while (!cancellationToken.IsCancellationRequested && searchRateLimit.Remaining > requestCount)
            {
                requestCount++;

                Trace.TraceInformation($"Fetching tweets #{ requestCount }");

                var fetchStartTime = DateTime.Now;

                var tweets = SearchTweets();

                var fetchDuration = DateTime.Now - fetchStartTime;

                Trace.TraceInformation($"Found { tweets.Count } tweets");

                if (tweets.Count > 0)
                    StoreTweetsAsync(tweets);

                if (minMillisBetweenRequests > fetchDuration.TotalMilliseconds)
                {
                    Trace.TraceInformation("Short snooze before next request");

                    Thread.Sleep(minMillisBetweenRequests - (int)fetchDuration.TotalMilliseconds);
                }
            }

            if (cancellationToken.IsCancellationRequested)
                Trace.TraceInformation("Twitter scraper cancelled");
            else
            {
                Trace.TraceInformation("Request limit reached. Short snooze before next run");

                Thread.Sleep(3000);
            }
        }

        private void InitializeLastTweetId()
        {
            if (_lastInsertedTweetId >= InitialTweetId)
                return;

            var lastInsertedTweetId = GetLatestStoredTweetId();

            _lastInsertedTweetId = lastInsertedTweetId ?? InitialTweetId;

            Trace.TraceInformation($"Initialized _lastInsertedTweetId to { _lastInsertedTweetId }");
        }

        private List<Tweet> SearchTweets()
        {
            var searchTweetsRequest = new SearchTweetsRequest
            {
                Count = 100,
                Geocode = new Geocode
                {
                    Latitude = 59.893855m,
                    Longitude = 10.7851165m,
                    Radius = 600,
                    Unit = GeocodeUnit.Km
                },
                SinceId = _lastInsertedTweetId,
                ResultType = ResultType.Recent
            };

            var tweetSearchResult = _twitterApi.SearchTweets(searchTweetsRequest);

            return tweetSearchResult.Tweets;
        }

        private async void StoreTweetsAsync(List<Tweet> tweets)
        {
            _lastInsertedTweetId = tweets.Max(tweet => tweet.Id);

            var tweetTableEntities = tweets.Select(tweet => new TweetTableEntity(tweet, true));
            var tweetTableEntities2 = tweets.Select(tweet => new TweetTableEntity(tweet, false));

            Trace.TraceInformation($"Tweets to store: { tweets.Count }");

            try
            {
                //insert both directions (oldest/newest first)
                var tableResults = await _tableStorage.BatchInsertEntitiesAsync(tweetTableEntities);
                var tableResults2 = await _secondTableStorage.BatchInsertEntitiesAsync(tweetTableEntities2);

                //if (CloudConfigurationManager.GetSetting("ShouldPublishEvent").Equals("true"))
                //{
                //    SendTweetsToTopic(tweetTableEntities);
                //}

                Trace.TraceInformation($"Successfully inserted entities: { tableResults.Count(result => result.HttpStatusCode == 200) }");
                Trace.TraceInformation($"Successfully inserted entities: { tableResults2.Count(result => result.HttpStatusCode == 200) }");
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error while inserting tweets: { e.Message }");
            }
        }

        //private async void SendTweetsToTopic(IEnumerable<TweetTableEntity> tweetTableEntities)
        //{
        //    foreach (var tweetTableEntity in tweetTableEntities)
        //    {
        //        var msg = new BrokeredMessage(tweetTableEntity.Tweet) {TimeToLive = TimeSpan.FromHours(1)};
        //        await _topicClient.SendAsync(msg);
        //    }
        //}

        private RateLimit GetSearchRateLimit()
        {
            var rateLimit = _twitterApi.GetRateLimits("search");

            return rateLimit.Resources.Search.SearchTweets;
        }

        private long? GetLatestStoredTweetId()
        {
            var topTweetTableEntity = _tableStorage.RetrieveEntities<TweetTableEntity>(1).FirstOrDefault();

            return topTweetTableEntity?.Tweet?.Id;
        }
    }
}
