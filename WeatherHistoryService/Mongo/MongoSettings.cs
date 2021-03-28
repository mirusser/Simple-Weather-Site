using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherHistoryService.Mongo
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; }
        public string database { get; set; }
        public string cityWeatherForecastsCollectionName { get; set; }
        public bool seed { get; set; }
    }
}
