using System;

namespace Entities.Weather
{
    /// <summary>
    ///     General weather result type
    /// </summary>
    public abstract class WeatherResult
    {
        /// <summary>
        ///     Time of data receiving in GMT.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        ///     City name.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        ///     Country name.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        ///     City identifier.
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        ///     WeatherResult title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     WeatherResult description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Temperature in Kelvin.
        /// </summary>
        public double Temp { get; set; }

        /// <summary>
        ///     Humidity in %
        /// </summary>
        public double Humidity { get; set; }

        /// <summary>
        ///     Maximum temperature in Kelvin.
        /// </summary>
        public double TempMax { get; set; }

        /// <summary>
        ///     Minimum temperature in Kelvin.
        /// </summary>
        public double TempMin { get; set; }

        /// <summary>
        ///     Wind speed in mps.
        /// </summary>
        public double WindSpeed { get; set; }

        /// <summary>
        ///     Icon name.
        /// </summary>
        public string Icon { get; set; }
    }
}