﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherHistoryService.Mongo.Documents;

namespace WeatherHistoryService.Models.Dto
{
    public class CityWeatherForecastPaginationDto
    {
        public List<CityWeatherForecastDocument> WeatherForecastDocuments { get; set; }
        public int NumberOfAllEntities { get; set; }
    }
}
