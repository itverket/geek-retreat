using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using Bing;
using Bing.Spatial;
using Entities.Twitter.SearchIndex;
using Microsoft.Azure;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using static System.String;

namespace TweetrSearchWeb.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string SearchServiceName = CloudConfigurationManager.GetSetting("SearchServiceName");
        private static readonly string ApiKey = CloudConfigurationManager.GetSetting("SearchServiceApiKey");

        private static readonly SearchServiceClient ServiceClient = new SearchServiceClient(SearchServiceName,
            new SearchCredentials(ApiKey));

        private readonly SearchIndexClient _indexClient = ServiceClient.Indexes.GetClient("tweets");

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Search(double? retweetCountFrom, double? retweetCountTo, string sort = null, string filter = null, string query = "", string hashTags = null, string username = null, int skip = 0,int top = 50, string scoringProfile = null,  string scoringParameter = null)
        {
            ViewBag.searchString = query;
            ViewBag.hashTags = hashTags;
            ViewBag.retweetCountFrom = retweetCountFrom;
            ViewBag.retweetCountTo = retweetCountTo;
            ViewBag.filter = filter;
            ViewBag.sort = sort;
            ViewBag.username = username;
            ViewBag.skip = skip;
            ViewBag.top = top;
            ViewBag.scoringProfile = scoringProfile;
            ViewBag.scoringParameter = scoringParameter;

            if (IsNullOrWhiteSpace(query))
            {
                return View("Index", null);
            }
            if (sort == "retweetCount")
            {
                sort = "retweetCount desc";
            }

            var searchParameters = BuildSearchParameters(retweetCountFrom, retweetCountTo, sort, filter, hashTags,username, top, skip, scoringProfile, scoringParameter);

            var response = _indexClient.Documents.Search<FlattendTweet>(query, searchParameters);
            return View("Index", response);
        }

        private static SearchParameters BuildSearchParameters(double? retweetCountFrom, double? retweetCountTo, string sort, string filter, string hashTags, string username, int top, int skip, string scoringProfile, string scoringParameter)
        {
            var sp = new SearchParameters
            {
                Top = top,
                Facets = new List<string>
                {
                    "retweetCount,values:10 | 25 | 50 | 100 | 250 | 1000",
                    "hashTags",
                    //"temperature, values:-20 | 0 | 20 | | 40",
                    //"cloudiness, values: 0 | 25 | 50 | 75 | 100"
                },
                OrderBy = new List<string> {sort},
                Filter = BuildSearchParameterFilter(retweetCountFrom, retweetCountTo, filter, hashTags, username),
                Skip = skip,
                IncludeTotalResultCount = true
            };
            if (!IsNullOrEmpty(scoringProfile))
            {
                sp.ScoringProfile = scoringProfile;
                if (!IsNullOrEmpty(scoringParameter))
                {
                    sp.ScoringParameters = new List<string>
                    {
                        scoringParameter
                    };
                }
            }
            return sp;
        }

        private static string BuildSearchParameterFilter(double? retweetCountFrom, double? retweetCountTo, string filter,
            string hashTags, string username)
        {
            var spFilter = string.Empty;
            if (retweetCountFrom.HasValue)
            {
                spFilter += Any(spFilter) + "retweetCount ge " +
                            retweetCountFrom.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (retweetCountTo.HasValue && retweetCountTo > 0)
            {
                spFilter += Any(spFilter) + "retweetCount le " +
                            retweetCountTo.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (!IsNullOrEmpty(hashTags))
            {
                spFilter += Any(spFilter) + "hashTags/any (h: h eq '" + hashTags + "')";
            }

            if (!IsNullOrEmpty(username))
            {
                spFilter += Any(spFilter) + "username eq '" + username + "'";
            }

            if (!IsNullOrEmpty(filter))
            {
                if (filter == "weather")
                {
                    spFilter += Any(spFilter) + "temperature gt -100";
                }
                if (filter == "hashTags" && IsNullOrEmpty(hashTags))
                {
                    spFilter += Any(spFilter) + "hashTags/any()";
                }
            }
            return spFilter;
        }

        private static string Any(string filter)
        {
            if (IsNullOrEmpty(filter)) return Empty;
            if (IsNullOrWhiteSpace(filter)) return Empty;
            return " and ";
        }

        [HttpGet]
        public ActionResult Suggest(string term)
        {
            return null;
        }

        private async void map()
        {
            // Take advantage of built-in Point of Interest groups
            var list = PoiEntityGroups.NightLife();
            list.Add(PoiEntityTypes.BarOrPub);

            // Build your filter list from the group.
            var filter = PoiEntityGroups.BuildFilter(list);

            var client = new SpatialDataClient("AkKZJ3NHO_6rRvfowxwcQTLNg7k68neuxQNxN2zMzsgwBZgHICjQUmS70CRRES4D");

            // All Bing results are in Kilometers, but convert them to Miles with our built-in conversion helper.
            var results = await client.Find<PointOfInterest>("EuropePOI", "Jernbanetorget 1, Oslo, Norway", client.ConvertMiToKm(3), filter, top: 10);
        }
    }
}
