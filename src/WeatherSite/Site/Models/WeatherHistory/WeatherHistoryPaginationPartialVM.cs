using System.Collections.Generic;
using WeatherSite.Logic.Clients.Models.Records;

namespace WeatherSite.Models.WeatherHistory;

public class WeatherHistoryPaginationPartialVM
{
    public PaginationVM PaginationVM { get; set; }

    //TODO: make a proper view model of this property
    public List<CityWeatherForecastDocument> CityWeatherForecastDocuments { get; set; }

    public string Url { get; set; }
}