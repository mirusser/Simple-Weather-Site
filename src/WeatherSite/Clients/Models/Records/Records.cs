using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherSite.Clients.Models.Records
{
    public record Coord(decimal Lon, decimal Lat);
    public record City(decimal Id, string Name, string State, string Country, Coord Coord);
    public record WeatherForecast(DateTime Date, int TemperatureC, int TemperatureF, string Summary);
    public record CitiesPagination(List<City> Cities, int NumberOfAllCities);
}
