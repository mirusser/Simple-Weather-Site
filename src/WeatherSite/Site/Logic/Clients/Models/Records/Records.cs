﻿using System;
using System.Collections.Generic;

namespace WeatherSite.Clients.Models.Records;

public record Coord(decimal Lon, decimal Lat);
public record Temperature(int TemperatureC, int TemperatureF);

public record City(decimal Id, string Name, string State, string Country, Coord Coord);
public record WeatherForecast(DateTime Date, int TemperatureC, int TemperatureF, string Summary, string Icon);
public record CitiesPagination(List<City> Cities, int NumberOfAllCities);

public record CityWeatherForecastDocument(string Id, string City, string CountryCode, DateTime SearchDate, Temperature Temperature, string Summary, string Icon);
public record WeatherHistoryForecastPagination(List<CityWeatherForecastDocument> WeatherForecastDocuments, int NumberOfAllEntities);

public record IconDto(string Name, string Description, bool DayIcon, byte[] FileContent, string Icon);