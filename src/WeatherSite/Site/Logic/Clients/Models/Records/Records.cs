using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherSite.Clients.Models.Records
{
    public record Coord(decimal Lon, decimal Lat);
    public record Temperature(int TemperatureC, int TemperatureF);

    public record City(decimal Id, string Name, string State, string Country, Coord Coord);
    public record WeatherForecast(DateTime Date, int TemperatureC, int TemperatureF, string Summary);
    public record CitiesPagination(List<City> Cities, int NumberOfAllCities);

    public record CityWeatherForecastDocument(Guid Id, string City, string CountryCode, DateTime SearchDate, Temperature Temperature, string Summary);
    public record WeatherHistoryForecastPagination(List<CityWeatherForecastDocument> WeatherForecastDocuments, int NumberOfAllEntities);

    public record IconDto(string Name, string Description, bool DayIcon, byte[] FileContent);
}
