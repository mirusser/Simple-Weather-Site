﻿using System.Collections.Generic;

namespace WeatherSite.Models.WeatherHistory;

public class WeatherHistoryPaginationPartialVM
{
    public PaginationVM PaginationVM { get; set; }

    //TODO: make a proper view model of this property
    public List<Clients.Models.Records.CityWeatherForecastDocument> CityWeatherForecastDocuments { get; set; }

    public string Url { get; set; }
}