using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace WeatherHistoryService.Mongo.Documents;

public class CityWeatherForecastDocument
{
    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("eventId")] public Guid EventId { get; set; }

    [Required(ErrorMessage = "City is required")]
    public string City { get; set; } = null!;

    [BsonElement("country code")]
    [Required(ErrorMessage = "Country code is required")]
    public string CountryCode { get; set; } = null!;

    [BsonElement("search date")]
    [Required(ErrorMessage = "Search date is required")]
    public DateTimeOffset SearchDate { get; set; }

    public Temperature Temperature { get; set; } = new();
    public string? Summary { get; set; }

    [BsonElement("Icon")]
    [BsonRepresentation(BsonType.String)]
    public string? Icon { get; set; }
}

public class Temperature
{
    public int TemperatureC { get; set; }
    public int TemperatureF { get; set; }
}