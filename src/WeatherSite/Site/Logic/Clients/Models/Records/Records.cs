using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WeatherSite.Logic.Clients.Models.Records;

public record CitiesResponse(
    [property: JsonProperty("cities")] List<City> Cities);

public record City(
    [property: JsonProperty("id")] decimal Id,
    [property: JsonProperty("name")] string Name,
    [property: JsonProperty("state")] string State,
    [property: JsonProperty("country")] string Country,
    [property: JsonProperty("coord")] Coord Coord);

public record CitiesPagination(List<City> Cities, int NumberOfAllCities);

public record Coord(
    [property: JsonProperty("lon")] decimal Lon,
    [property: JsonProperty("lat")] decimal Lat);

public record Temperature(int TemperatureC, int TemperatureF);
public record WeatherForecast(DateTime Date, int TemperatureC, int TemperatureF, string Summary, string Icon);

public record CityWeatherForecastDocument(string Id, string City, string CountryCode, DateTime SearchDate, Temperature Temperature, string Summary, string Icon);

public record WeatherHistoryForecastPagination(List<CityWeatherForecastDocument> WeatherForecastDocuments, int NumberOfAllEntities);

public record IconDto(string Name, string Description, bool DayIcon, byte[] FileContent, string Icon);