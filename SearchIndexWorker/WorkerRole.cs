using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Entities.Twitter.Tweet;
using Microsoft.Azure;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace SearchIndexWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);
        private string _apiKey;
        private CloudQueueClient _queueClient;
        private SearchIndex _searchIndex;
        private SearchIndexClient _searchIndexClient;
        private CloudQueue _searchIndexQueue;
        private SearchServiceClient _searchServiceClient;
        private string _searchServiceName;
        private string _shouldPublishIndex;
        private CloudStorageAccount _storageAccount;
        private TweetsEnricher _tweetsEnricher;

        private static int MessageIndexCount => 10;

        public override bool OnStart()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            _shouldPublishIndex = CloudConfigurationManager.GetSetting("ShouldPublishIndex");

            _tweetsEnricher = new TweetsEnricher();
            _searchIndex = new SearchIndex();

            InitializeSearchService();
            InitializeSearchIndexQueue();

            if (_shouldPublishIndex == "true")
            {
                _searchIndex.CreateOrUpdateSearchIndex(_searchServiceClient);
            }

            ServicePointManager.DefaultConnectionLimit = 12;
            var result = base.OnStart();
            Trace.TraceInformation("SearchIndexWorker has been started");
            return result;
        }

        public override void Run()
        {
            Trace.TraceInformation("SearchIndexWorker is running");

            var interval = 1;
            const int maxSleep = 20;
            const int exponent = 2;

            var cancellationToken = _cancellationTokenSource.Token;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var tweets = GetTweetsFromMessageQueue();
                    if (tweets.Count > 0 && _shouldPublishIndex == "true")
                    {
                        UploadTweetsToSearchIndex(tweets);
                        interval = 1;
                    }
                    else
                    {
                        Trace.TraceInformation("Sleeping for {0} seconds", interval);
                        Thread.Sleep(TimeSpan.FromSeconds(interval));
                        interval = Math.Min(maxSleep, interval*exponent);
                    }
                }
            }
            finally
            {
                _runCompleteEvent.Set();
            }
        }

        private List<Tweet> GetTweetsFromMessageQueue()
        {
            var tweets = new List<Tweet>();
            foreach (var msg in _searchIndexQueue.GetMessages(MessageIndexCount).Where(msg => msg != null))
            {
                var messageString = msg.AsString;
                tweets.Add(JsonConvert.DeserializeObject<Tweet>(messageString));
                _searchIndexQueue.DeleteMessage(msg);
            }
            return tweets;
        }

        private void UploadTweetsToSearchIndex(List<Tweet> tweets)
        {
            if (_searchIndexClient == null)
            {
                _searchIndexClient = _searchServiceClient.Indexes.GetClient("tweets");
            }

            var enrichedTweets = _tweetsEnricher.FlattenAndEnrichTweets(tweets);

            try
            {
                _searchIndexClient.Documents.Index(IndexBatch.Create(enrichedTweets.Select(IndexAction.Create)));
                Trace.TraceInformation($"Uploaded indexes for tweets: {string.Join(", ", enrichedTweets.Select(t => t.TweetId))}.");
            }
            catch (IndexBatchException e)
            {
                Trace.TraceError("Failed to index some tweets: {0}", string.Join(", ", e.IndexResponse.Where(r => !r.Succeeded).Select(r => r.Key)));
            }

            //Make it slow
            Thread.Sleep(200);
        }

        private void InitializeSearchService()
        {
            _searchServiceName = CloudConfigurationManager.GetSetting("SearchServiceName");
            _apiKey = CloudConfigurationManager.GetSetting("SearchApiKey");
            _searchServiceClient = new SearchServiceClient(_searchServiceName, new SearchCredentials(_apiKey));
        }

        private void InitializeSearchIndexQueue()
        {
            _queueClient = _storageAccount.CreateCloudQueueClient();
            _searchIndexQueue = _queueClient.GetQueueReference("searchindexqueue");
            _searchIndexQueue.CreateIfNotExists();
        }

        public override void OnStop()
        {
            Trace.TraceInformation("SearchIndexWorker is stopping");

            _cancellationTokenSource.Cancel();
            _runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("SearchIndexWorker has stopped");
        }
    }
}