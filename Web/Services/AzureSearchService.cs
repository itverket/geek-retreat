using Entities.Twitter.SearchIndex;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Framework.Configuration;
using System.Collections.Generic;
using Web.ViewModels;

namespace Web.Services
{
    public class AzureSearchService
    {
        private SearchIndexClient _indexClient;

        public AzureSearchService(IConfigurationRoot configurationRoot)
        {
            var searchServiceName = configurationRoot["AppSettings:SearchServiceName"];
            var searchApiKey = configurationRoot["AppSettings:SearchServiceApiKey"];
            var tweetIndexName = configurationRoot["AppSettings:TweetIndexName"];

            var serviceClient = new SearchServiceClient(
                searchServiceName, new SearchCredentials(searchApiKey));

            _indexClient = serviceClient.Indexes.GetClient(tweetIndexName);
        }

        public virtual AzureSearchResult Search(
            string query, string username = null)
        {
            var result = _indexClient.Documents.Search<FlattendTweet>(
                query,
                new SearchParameters
                {
                    OrderBy = new List<string> { "CreatedAt" },
                    Filter = string.IsNullOrEmpty(username)
                        ? null
                        : $"Username eq '{username}'"
                });

            return new AzureSearchResult
            {
                Count = result.Count,
                Facets = result.Facets,
                Tweets = result.Results
            };
        }
    }
}
