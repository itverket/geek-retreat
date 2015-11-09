using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Entities.Twitter.Tweet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WeatherUndergroundService.Util.Data
{
    public static class CityHelper
    {
        public static Dictionary<double, City> Cities = new Dictionary<double, City>();

        public static void LoadCities()
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "city.list.json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return;
                using (var r = new StreamReader(stream))
                {
                    var cities = JsonConvert.DeserializeObject<List<City>>(r.ReadToEnd());
                    foreach (var city in cities.Where(city => !Cities.ContainsKey(city.Id)))
                    {
                        Cities.Add(city.Id, city);
                    }
                }
            }
        }


    }
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public JsonCoord Coord { get; set; }

    }

    public class JsonCoord
    {
        public double Lon { get; set; }
        public double Lat { get; set; }
    }
}