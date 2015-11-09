using System;
using System.Collections.Generic;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace SearchIndexWorker
{
    public class SearchIndex
    {
        public void CreateOrUpdateSearchIndex(ISearchServiceClient searchServiceClient)
        {
            var index = CreateSearchIndex();
            searchServiceClient.Indexes.CreateOrUpdate(index);
        }

        private static Index CreateSearchIndex()
        {
            return new Index()
            {
                Name = "tweets",
                Fields = new[]
                {
                    new Field("tweetId",            DataType.String) {IsKey = true},
                    new Field("username",           DataType.String) {IsSearchable = true, IsFilterable = true},
                    new Field("tweetMessage",       DataType.String, AnalyzerName.EnLucene) {IsSearchable = true, IsFilterable = true},
                    new Field("date",               DataType.String) {IsSearchable = true, IsFilterable = true, IsSortable = true},
                    new Field("createdAt",          DataType.DateTimeOffset) {IsSortable = true, IsFilterable = true},
                    new Field("retweetCount",       DataType.Int32) { IsFacetable = true, IsFilterable = true, IsSortable = true},
                    new Field("tweetCoordinates",   DataType.GeographyPoint) {IsFilterable = true},
                    new Field("weatherId",          DataType.String) {IsSortable =  false, IsSearchable = false, IsFacetable = false, IsFilterable = false, IsRetrievable = true}, 
                    new Field("weatherDescription", DataType.String) {IsSearchable = true},
                    new Field("weatherTitle",       DataType.String) {IsSearchable = true, IsFacetable = true, IsFilterable = true},
                    new Field("cloudiness",         DataType.Double) {IsFilterable = true, IsFacetable = true, IsSortable = true},
                    new Field("temperature",        DataType.Double) {IsSearchable = false, IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new Field("humidity",           DataType.Double) {IsSearchable = false, IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new Field("windSpeed",          DataType.Double) {IsSearchable = false, IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new Field("windDegree",         DataType.Double) {IsSearchable = false, IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new Field("iconUrl",            DataType.String) {IsRetrievable = true},
                    new Field("hashTags",           DataType.Collection(DataType.String)) {IsFacetable = true, IsSearchable = true, IsFilterable = true},
                    new Field("urls",               DataType.Collection(DataType.String)) {IsFacetable = false, IsSearchable = false, IsFilterable = false, IsSortable = false, IsRetrievable = true},
                },
                ScoringProfiles = CreateScoringProfiles()
            };
        }

        private static List<ScoringProfile> CreateScoringProfiles()
        {
            var freshnessScoringFunction = new FreshnessScoringFunction
            {
                Boost = 2.5,
                FieldName = "createdAt",
                Parameters = new FreshnessScoringParameters(new TimeSpan(7, 0, 0)),
            };

            var geekScoringFunction = new TagScoringFunction
            {
                Boost = 10,
                FieldName = "tweetMessage",
                Parameters = new TagScoringParameters("geekScore")
            };
            var distanceScoringFunction = new DistanceScoringFunction
            {
                Boost = 2.5,
                FieldName = "tweetCoordinates",
                Parameters = new DistanceScoringParameters("clientLocation", 100)
            };
            var retweetCountScoringFunction = new MagnitudeScoringFunction
            {
                Boost = 2.5,
                FieldName = "retweetCount",
                Parameters = new MagnitudeScoringParameters(10, 100000)
            };

            var freshGeeksProfile = new ScoringProfile
            {
                Name = "FreshGeeks",
                Functions = new List<ScoringFunction>
                {
                    freshnessScoringFunction,
                    geekScoringFunction,

                }
            };
            var neighbourGeeksProfile = new ScoringProfile
            {
                Name = "GeeklyNeighbours",
                Functions = new List<ScoringFunction>
                {
                    distanceScoringFunction,
                    geekScoringFunction,
                }
            };
            var popularGeeksProfile = new ScoringProfile
            {
                Name = "PopularGeeks",
                Functions = new List<ScoringFunction>
                {
                    retweetCountScoringFunction,
                    geekScoringFunction
                }
            };

            return new List<ScoringProfile>
            {
                freshGeeksProfile,
                neighbourGeeksProfile,
                popularGeeksProfile
            };
        }
    }
}