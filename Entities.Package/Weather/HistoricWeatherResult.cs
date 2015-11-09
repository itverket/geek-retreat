using System;

namespace Entities.Weather
{
    /// <summary>
    ///     HistoricWeatherResult weather result type.
    /// </summary>
    public class HistoricWeatherResult : WeatherResult
    {
        /// <summary>
        ///     Time of data receiving in unixtime GMT.
        /// </summary>
        public int DateUnixFormat { get; set; }

        /// <summary>
        ///     Cloudiness in %
        /// </summary>
        public double Clouds { get; set; }

        public string WeatherId { get; set; }
        public double WindDegree { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
    }
}