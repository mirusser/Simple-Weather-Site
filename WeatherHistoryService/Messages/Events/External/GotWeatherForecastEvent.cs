using Convey.CQRS.Events;
using Convey.MessageBrokers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WeatherHistoryService.Models.Dto;

namespace WeatherHistoryService.Messages.Events.External
{
    [Message("weatherservice")]
    public class GotWeatherForecastEvent : IEvent
    {
        public WeatherForecastDto WeatherForecastDto { get; set; }

        [JsonConstructor]
        public GotWeatherForecastEvent(WeatherForecastDto weatherForecastDto)
        {
            WeatherForecastDto = weatherForecastDto;
        }
    }
}
