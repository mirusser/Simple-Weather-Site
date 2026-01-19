using System;

namespace MQModels.WeatherHistory;

public record CreatedCityWeatherForecastSearch(
    Guid EventId,
    Guid GotWeatherForecastEventIt);