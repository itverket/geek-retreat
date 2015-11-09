using System;
using System.Collections.Generic;
using System.Globalization;
using Entities.Weather;
using WeatherUndergroundService.Util.Api;
using WeatherUndergroundService.Util.Data;

namespace WeatherUndergroundService.Clients
{
    public class HistoricWeather
    {
        /// <summary>
        ///     Get the historic data for a specific city by indicating its 'OpenwWeatherMap' identifier, start date, and end 
        ///     date or count
        /// </summary>
        /// <param name="id">City 'OpenwWeatherMap' identifier.</param>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <param name="count">Amount of returned data (one per hour, can be used instead of 'end')</param>
        /// <returns> The historic information.</returns>
        public static IList<HistoricWeatherResult> GetByCityId(int id, DateTime start, DateTime? end=null, int? count=null)
        {
            try
            {
                if (0 > id)
                    return new List<HistoricWeatherResult>();
                if (!end.HasValue && !count.HasValue)
                    return new List<HistoricWeatherResult>();

                var queryString = "/history/city?id=" + id + "&type=hour&start" + TimeHelper.ToUnixTimestamp(start);
                if (end.HasValue)
                    queryString += "&end=" + TimeHelper.ToUnixTimestamp(end.Value);
                else
                    queryString += "&cnt=" + count.Value;


                var response = HistoryApiClient.GetResponse(queryString);
                return Deserializer.GetWeatherHistoric(response);
            }
            catch (Exception ex)
            {
                return new List<HistoricWeatherResult>();
            }
        }

        /// <summary>
        ///     Get the historic data for a specific city by indicating the city and country names.
        /// </summary>
        /// <param name="city">Name of the city.</param>
        /// <param name="country">Country of the city.</param>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <param name="count">Amount of returned data (one per hour, can be used instead of 'end')</param>
        /// <returns> The forecast information.</returns>
        public static IList<HistoricWeatherResult> GetByCityName(string city, string country, DateTime start, DateTime? end = null, int? count = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(city) || string.IsNullOrEmpty(country))
                    return new List<HistoricWeatherResult>();
                if(!end.HasValue && !count.HasValue)
                    return new List<HistoricWeatherResult>();

                var queryString = "/history/city?q=" + city + "," + country + "&type=hour&start" + TimeHelper.ToUnixTimestamp(start);
                if (end.HasValue)
                    queryString += "&end=" + TimeHelper.ToUnixTimestamp(end.Value);
                else
                    queryString += "&cnt=" + count.Value;

                var response = HistoryApiClient.GetResponse(queryString);

                return Deserializer.GetWeatherHistoric(response);
            }
            catch (Exception ex)
            {
                return new List<HistoricWeatherResult>();
            }
        }

        /// <summary>
        ///     Get the historic data for a specificcity by indicating its coordinates.
        /// </summary>
        /// <param name="lat">Latitud of the city.</param>
        /// <param name="lon">Longitude of the city.</param>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <param name="count">Amount of returned data (one per hour, can be used instead of 'end')</param>/// 
        /// <returns> The historic information.</returns>
        public static IList<HistoricWeatherResult> GetByCoordinates(double lat, double lon, DateTime start, DateTime? end = null, int? count = null)
        {

            try
            {
                if (!end.HasValue && !count.HasValue)
                    return new List<HistoricWeatherResult>();

                var queryString = string.Format(CultureInfo.InvariantCulture, "/history/city?lat={0}&lon={1}&type=hour&start={2}",lat,lon,TimeHelper.ToUnixTimestamp(start));
                if (end.HasValue)
                    queryString += "&end=" + TimeHelper.ToUnixTimestamp(end.Value);
                else
                    queryString += "&cnt=" + count.Value;

                var response = HistoryApiClient.GetResponse(queryString);

                return Deserializer.GetWeatherHistoric(response);
            }
            catch (Exception ex)
            {
                return new List<HistoricWeatherResult>();
            }
        }
    }
}