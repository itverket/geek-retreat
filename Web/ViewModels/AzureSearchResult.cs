using Entities.Twitter.SearchIndex;
using Microsoft.Azure.Search.Models;
using System.Collections.Generic;

namespace Web.ViewModels
{
    public class AzureSearchResult
    {
        public long? Count { get; set; }
        public FacetResults Facets { get; set; }
        public IList<SearchResult<FlattendTweet>> Tweets { get; set; }
    }
}
