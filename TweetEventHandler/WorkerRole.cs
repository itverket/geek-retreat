using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Entities.Twitter.Tweet;
using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Storage.Azure.Table.Entities.Twitter;

namespace TweetEventHandler
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);
        private CloudStorageAccount _storageAccount;
        private CloudQueueClient _queueClient;
        private CloudQueue _searchIndexQueue;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;
        private CloudQueue _tweetQueue;
        private CloudTable _tweetsTable;
        private CloudTableClient _tableClient;

        public override bool OnStart()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            InitializeSearchIndexQueue();
            InitializeTweetQueue();
            InitializeTweetsTable();
            InitializeBlobContainer();

            ServicePointManager.DefaultConnectionLimit = 12;
            bool result = base.OnStart();
            Trace.TraceInformation("TweetEventHandler has been started");
            return result;
        }
        public override void Run()
        {
            Trace.TraceInformation("TweetEventHandler is running");

            try
            {
                RunAsync(_cancellationTokenSource.Token).Wait();
            }
            finally
            {
                _runCompleteEvent.Set();
            }
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {

            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Running Tweets subscriber");
                await Task.Factory.StartNew(() => ReceiveTweetsFromQueueWithBackoff(cancellationToken), cancellationToken);
            }
        }

        private async void ReceiveTweetsFromQueueWithBackoff(CancellationToken cancellationToken)
        {
            var interval = 1;
            const int maxSleep = 20;
            const int exponent = 2;
            while (!cancellationToken.IsCancellationRequested)
            {
                var msg = _tweetQueue.GetMessage();
                if (msg != null)
                {
                    var tweetString = msg.AsString;
                    var tweet = JsonConvert.DeserializeObject<Tweet>(tweetString);
                    var taskList = new List<Task>();

                    Trace.TraceInformation("Handling tweet message with id: {0}. Uploading to blob and adding to indexing queue.", tweet.IdString);
                    taskList.Add(UploadTweetToBlobAsync(tweet));
                    taskList.Add(_searchIndexQueue.AddMessageAsync(new CloudQueueMessage(msg.AsString), cancellationToken));

                    await Task.WhenAll(taskList.ToArray());
                    _tweetQueue.DeleteMessage(msg);
                    interval = 1;
                }
                else
                {
                    Trace.TraceInformation("Sleeping for {0} seconds", interval);
                    Thread.Sleep(TimeSpan.FromSeconds(interval));
                    interval = Math.Min(maxSleep, interval * exponent);
                }
            }
        }

        private void InitializeTweetsTable()
        {
            _tableClient = _storageAccount.CreateCloudTableClient();
            _tweetsTable = _tableClient.GetTableReference("grtweets");
            _tweetsTable.CreateIfNotExists();
        }

        private void InitializeTweetQueue()
        {
            _tweetQueue = _queueClient.GetQueueReference("tweetsqueue");
            _tweetQueue.CreateIfNotExists();
        }

        private void InitializeBlobContainer()
        {
            _blobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference("tweetscontainer");
            _blobContainer.CreateIfNotExists();
        }

        private void InitializeSearchIndexQueue()
        {
            _queueClient = _storageAccount.CreateCloudQueueClient();
            _searchIndexQueue = _queueClient.GetQueueReference("searchindexqueue");
            _searchIndexQueue.CreateIfNotExists();
        }


        public override void OnStop()
        {
            Trace.TraceInformation("TweetEventHandler is stopping");

            _cancellationTokenSource.Cancel();
            _runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("TweetEventHandler has stopped");
        }


        private async Task UploadTweetToBlobAsync(Tweet tweet)
        {
            CloudBlockBlob tweetBlob = _blobContainer.GetBlockBlobReference(tweet.IdString);
            using (var ms = new MemoryStream())
            {
                var writer = new StreamWriter(ms);
                writer.Write(JsonConvert.SerializeObject(tweet));
                writer.Flush();
                ms.Position = 0;

                await tweetBlob.UploadFromStreamAsync(ms);
            }
        }
        private void UploadTweetToTable(Tweet tweet)
        {
            var insertOperation = TableOperation.InsertOrReplace(new TweetTableEntity(tweet, false));
            _tweetsTable.Execute(insertOperation);
            Trace.TraceInformation("Successfully uploaded tweet to tablestorage");
        }
    }
}
