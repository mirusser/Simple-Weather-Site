using Convey.Types;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherHistoryService.Mongo.Documents
{
    public class Temperature
    {
        public int TemperatureC { get; set; }
        public int TemperatureF { get; set; }
    }

    public class CityWeatherForecastDocument 
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Search date is required")]
        public DateTime SearchDate { get; set; }

        public Temperature Temperature { get; set; }
        public string Summary { get; set; }
    }
}
