using Convey.CQRS.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherService.Models.Dto;

namespace WeatherService.Messages.Events
{
    public class GotWeatherForecastEvent : IEvent
    {
        public WeatherForecastDto WeatherForecastDto { get; set; }

        public GotWeatherForecastEvent(WeatherForecastDto weatherForecastDto)
        {
            WeatherForecastDto = weatherForecastDto;
        }
    }
}
