using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Configuration;
using System.Xml;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Storage.Azure.Table;
using Storage.Azure.Table.Entities.Twitter;

namespace Tweetr
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);

        private readonly TwitterScraper _twitterScraper;

        public WorkerRole()
        {
            var twitterApi = InitializeTwitterApi();
            var tableStorage = InitializeAzureTableStorage();
            var secondTableStorage = InitializeAzureTableStorage2();
            //var topicClient = InitializeTopicClient(); 
            _twitterScraper = new TwitterScraper(twitterApi, tableStorage, secondTableStorage);
        }

        //private static TopicClient InitializeTopicClient()
        //{
        //    var serviceBusConnectionString = CloudConfigurationManager.GetSetting("ServiceBusNamespace");
        //    //var serviceBusConnectionString =
        //    //    "Endpoint=sb://grservicebusns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=J2yuB29cO+ocvtLvAnR82wINjoKkucdjj2bWRt3ZX2I=";
        //    var namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);
        //    if (!namespaceManager.TopicExists("TweetTopic"))
        //    {
        //        namespaceManager.CreateTopic("TweetTopic");
        //    }
        //    //TODO: Create individual subscriptions for all the groups and distribute a connectionString with only read permissions or let each group manage topics.
        //    if (!namespaceManager.SubscriptionExists("TweetTopic", "AllTweets"))
        //    {
        //        namespaceManager.CreateSubscription("TweetTopic", "AllTweets");
        //    }
        //    var client = TopicClient.CreateFromConnectionString(serviceBusConnectionString, "TweetTopic");

        //    return client;
        //}


        private static TwitterApi InitializeTwitterApi()
        {
            var consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
            var consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];

            return new TwitterApi(consumerKey, consumerSecret);
        }

        private static AzureTableStorage InitializeAzureTableStorage()
        {
            var azureTableStorageConnectionString = CloudConfigurationManager.GetSetting("AzureTableStorage");

            var tableStorage = new AzureTableStorage(azureTableStorageConnectionString, TweetTableEntity.TweetTableName);
            tableStorage.CreateIfNotExists();

            return tableStorage;
        }

        private static AzureTableStorage InitializeAzureTableStorage2()
        {
            var azureTableStorageConnectionString = CloudConfigurationManager.GetSetting("AzureTableStorage2");

            var tableStorage = new AzureTableStorage(azureTableStorageConnectionString, "tweetsoldestfirst");
            tableStorage.CreateIfNotExists();

            return tableStorage;
        }
        public override void Run()
        {
            Trace.TraceInformation("Tweetr is running");

            try
            {
                RunAsync(_cancellationTokenSource.Token).Wait();
            }
            finally
            {
                _runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            bool result = base.OnStart();

            Trace.TraceInformation("Tweetr has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("Tweetr is stopping");

            _cancellationTokenSource.Cancel();
            _runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("Tweetr has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Running Twitter Scraper");

                await _twitterScraper.RunAsync(cancellationToken);
            }
        }
    }
}
