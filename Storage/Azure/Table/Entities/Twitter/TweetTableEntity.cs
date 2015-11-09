using Entities.Twitter.Tweet;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Storage.Azure.Table.Entities.Twitter
{
    public class TweetTableEntity : TableEntity
    {
        public const string TweetPartitionKey = "tweet";
        public const string TweetTableName = "tweets";

        public Tweet Tweet => TweetJson == null ? null : JsonConvert.DeserializeObject<Tweet>(TweetJson);

        public string TweetJson { get; set; }

        public TweetTableEntity()
        {
        }

        public TweetTableEntity(Tweet tweet, bool flipId)
        {
            PartitionKey = TweetPartitionKey;
            RowKey = flipId ? string.Format("{0:d19}", long.MaxValue - tweet.Id) : string.Format("{0:d19}", tweet.Id);

            TweetJson = JsonConvert.SerializeObject(tweet);
        }
    }
}