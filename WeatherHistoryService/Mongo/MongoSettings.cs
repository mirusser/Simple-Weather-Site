using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherHistoryService.Mongo
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string CityWeatherForecastsCollectionName { get; set; }
        public bool Seed { get; set; }
    }
}
