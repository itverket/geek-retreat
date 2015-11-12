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
                Name = "asp5tweets",
                Fields = new[]
                {
                    new Field("TweetId",            DataType.String) {IsKey = true},
                    new Field("Username",           DataType.String) {IsSearchable = true, IsFilterable = true},
                    new Field("TweetMessage",       DataType.String, AnalyzerName.EnLucene) {IsSearchable = true, IsFilterable = true},
                    new Field("Date",               DataType.String) {IsSearchable = true, IsFilterable = true, IsSortable = true},
                    new Field("CreatedAt",          DataType.DateTimeOffset) {IsSortable = true, IsFilterable = true},
                    new Field("RetweetCount",       DataType.Int32) { IsFacetable = true, IsFilterable = true, IsSortable = true},
                    new Field("TweetCoordinates",   DataType.GeographyPoint) {IsFilterable = true, IsSortable = true},
                    new Field("WeatherId",          DataType.String) {IsSortable =  false, IsSearchable = false, IsFacetable = false, IsFilterable = false, IsRetrievable = true}, 
                    new Field("WeatherDescription", DataType.String) {IsSearchable = true},
                    new Field("WeatherTitle",       DataType.String) {IsSearchable = true, IsFacetable = true, IsFilterable = true},
                    new Field("Cloudiness",         DataType.Double) {IsFilterable = true, IsFacetable = true, IsSortable = true},
                    new Field("Temperature",        DataType.Double) {IsSearchable = false, IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new Field("Humidity",           DataType.Double) {IsSearchable = false, IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new Field("WindSpeed",          DataType.Double) {IsSearchable = false, IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new Field("WindDegree",         DataType.Double) {IsSearchable = false, IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new Field("IconUrl",            DataType.String) {IsRetrievable = true},
                    new Field("HashTags",           DataType.Collection(DataType.String)) {IsFacetable = true, IsSearchable = true, IsFilterable = true},
                    new Field("Urls",               DataType.Collection(DataType.String)) {IsFacetable = false, IsSearchable = false, IsFilterable = false, IsSortable = false, IsRetrievable = true},
                },
                ScoringProfiles = CreateScoringProfiles()
            };
        }

        private static List<ScoringProfile> CreateScoringProfiles()
        {
            var freshnessScoringFunction = new FreshnessScoringFunction
            {
                Boost = 10,
                FieldName = "CreatedAt",
                Parameters = new FreshnessScoringParameters(new TimeSpan(7, 0, 0)),
                Interpolation = ScoringFunctionInterpolation.Quadratic
            };

            var distanceScoringFunction = new DistanceScoringFunction
            {
                Boost = 10,
                FieldName = "TweetCoordinates",
                Parameters = new DistanceScoringParameters("clientLocation", 3)
            };
            var retweetCountScoringFunction = new MagnitudeScoringFunction
            {
                Boost = 10,
                FieldName = "RetweetCount",
                Parameters = new MagnitudeScoringParameters(10, 1000),
                Interpolation = ScoringFunctionInterpolation.Logarithmic
            };

            var freshGeeksProfile = new ScoringProfile
            {
                Name = "FreshGeeks",
                Functions = new List<ScoringFunction>
                {
                    freshnessScoringFunction

                },
            };
            var neighbourGeeksProfile = new ScoringProfile
            {
                Name = "GeeklyNeighbours",
                Functions = new List<ScoringFunction>
                {
                    distanceScoringFunction
                }
            };
            var popularGeeksProfile = new ScoringProfile
            {
                Name = "PopularGeeks",
                Functions = new List<ScoringFunction>
                {
                    retweetCountScoringFunction
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