using AutoMapper;
using Convey.CQRS.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Messages.Events.External.Handlers
{
    public class GotWeatherForecastHandler : IEventHandler<GotWeatherForecastEvent>
    {
        private readonly ICityWeatherForecastService _cityWeatherForecastService;
        private readonly IMapper _mapper;

        public GotWeatherForecastHandler(
            ICityWeatherForecastService cityWeatherForecastService,
            IMapper mapper)
        {
            _cityWeatherForecastService = cityWeatherForecastService;
            _mapper = mapper;
        }

        public async Task HandleAsync(GotWeatherForecastEvent @event)
        {
            try
            {
                if (@event != null && @event.WeatherForecastDto != null)
                {
                    var cityWeatherForecastDocument = _mapper.Map<CityWeatherForecastDocument>(@event.WeatherForecastDto);
                    var foo = await _cityWeatherForecastService.CreateAsync(cityWeatherForecastDocument);
                }
            }
            catch (Exception ex)
            {
                var c = ex;
            }

            await Task.CompletedTask;
        }
    }
}
