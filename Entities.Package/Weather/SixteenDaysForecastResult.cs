namespace Entities.Weather
{
    /// <summary>
    ///     SixteenDaysForecastResult forecast result type.
    /// </summary>
    public class SixteenDaysForecastResult : WeatherResult
    {
        /// <summary>
        ///     Temperature in the morning in Kelvin.
        /// </summary>
        public double TempMorning { get; set; }

        /// <summary>
        ///     Temperature in the evening in Kelvin.
        /// </summary>
        public double TempEvening { get; set; }

        /// <summary>
        ///     Temperature at night in Kelvin.
        /// </summary>
        public double TempNight { get; set; }

        /// <summary>
        ///     Atmospheric pressure in hPa.
        /// </summary>
        public double Pressure { get; set; }

        /// <summary>
        ///     Precipitation volume mm per 3 hours.
        /// </summary>
        public double Rain { get; set; }

        /// <summary>
        ///     Time of data receiving in unixtime GMT.
        /// </summary>
        public int DateUnixFormat { get; set; }

        /// <summary>
        ///     Cloudiness in %
        /// </summary>
        public double Clouds { get; set; }
    }
}