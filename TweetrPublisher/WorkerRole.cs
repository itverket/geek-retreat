using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.Azure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Storage.Azure.Table.Entities.Twitter;

namespace TweetrPublisher
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);
        private CloudStorageAccount _tweetsQueueStorageAccount;
        private CloudQueueClient _queueClient;
        private CloudQueue _tweetQueue;
        private CloudStorageAccount _tweetrStorageAccount;
        private CloudTable _tweetsTable;
        int _tweetsPerMinute;
        private CloudTableClient _tableClient;


        public override bool OnStart()
        {
            SetTweetPublishRate();

            //storage account used by tweetr to store tweets. (Comman configuration for all teams, do not change)
            _tweetrStorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("TweetrConnectionString"));
            //storage account of this cloud service (specific to each team)
            _tweetsQueueStorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            InitializeTweetSourceTable();
            InitializeTweetDestinationQueue();

            ServicePointManager.DefaultConnectionLimit = 12;
            RoleEnvironment.Changed += RoleEnvironment_Changed;

            bool result = base.OnStart();
            Trace.TraceInformation("TweetrPublisher has been started");

            return result;
        }

        public override void Run()
        {
            Trace.TraceInformation("TweetrPublisher is running");

            try
            {
                TableQuery query = new TableQuery();
                TableContinuationToken token = null;
                var cancellationToken = _cancellationTokenSource.Token;
                while (!cancellationToken.IsCancellationRequested)
                {
                    var segment = Get1000Tweets(query, ref token);

                    foreach (var tweetTableEntity in segment.Results)
                    {
                        var tweetJson = tweetTableEntity.TweetJson;
                        _tweetQueue.AddMessage(new CloudQueueMessage(tweetJson));
                        Trace.TraceInformation("Published tweet with id {1} to queue, {0} seconds until next.", 60000 / (_tweetsPerMinute * 1000), tweetTableEntity.Tweet.IdString);
                        Thread.Sleep(60000 / _tweetsPerMinute);
                    }
                }
                //RunAsync(_cancellationTokenSource.Token).Wait();
            }
            finally
            {
                _runCompleteEvent.Set();
            }
        }

        private void RoleEnvironment_Changed(object sender, RoleEnvironmentChangedEventArgs e)
        {
            var configChanges = e.Changes
                .OfType<RoleEnvironmentConfigurationSettingChange>()
                .ToList();

            if (!configChanges.Any())
                return;

            if (configChanges.Any(c => c.ConfigurationSettingName == "TweetsPerMinute"))
            {
                Trace.TraceInformation("Role service configuration changed, updating the publish rate for role.");
                SetTweetPublishRate();
            }
        }

        private void SetTweetPublishRate()
        {
            _tweetsPerMinute = Convert.ToInt32(CloudConfigurationManager.GetSetting("TweetsPerMinute"));
        }
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            TableQuery query = new TableQuery();
            TableContinuationToken token = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                var segment = Get1000Tweets(query, ref token);

                foreach (var tweetTableEntity in segment.Results)
                {
                    var tweetJson = tweetTableEntity.TweetJson;
                    await _tweetQueue.AddMessageAsync(new CloudQueueMessage(tweetJson), cancellationToken);
                    Trace.TraceInformation("Published tweet with id {1} to queue, {0} seconds until next.", 60000 / (_tweetsPerMinute * 1000), tweetTableEntity.Tweet.IdString);
                    Thread.Sleep(60000 / _tweetsPerMinute);
                }

            }
        }

        private void InitializeTweetDestinationQueue()
        {
            _queueClient = _tweetsQueueStorageAccount.CreateCloudQueueClient();
            _tweetQueue = _queueClient.GetQueueReference("tweetsqueue");
            _tweetQueue.CreateIfNotExists();
        }

        private void InitializeTweetSourceTable()
        {
            _tableClient = _tweetrStorageAccount.CreateCloudTableClient();
            _tweetsTable = _tableClient.GetTableReference("tweets");
        }


        private TableQuerySegment<TweetTableEntity> Get1000Tweets(TableQuery query, ref TableContinuationToken token)
        {
            EntityResolver<TweetTableEntity> tweetTableEntityResolver = (pk, rk, ts, props, etag) =>
            {
                TweetTableEntity resolvedEntity = new TweetTableEntity
                {
                    PartitionKey = pk,
                    RowKey = rk,
                    Timestamp = ts,
                    ETag = etag,
                };

                resolvedEntity.ReadEntity(props, null);

                return resolvedEntity;
            };
            var segment = _tweetsTable.ExecuteQuerySegmented<TweetTableEntity>(query, tweetTableEntityResolver, token);
            token = segment.ContinuationToken;
            return segment;
        }



        public override void OnStop()
        {
            Trace.TraceInformation("TweetrPublisher is stopping");

            this._cancellationTokenSource.Cancel();
            this._runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("TweetrPublisher has stopped");
        }

    }
}
