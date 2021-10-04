using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using MQModels.WeatherHistory;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Listeners
{
    public class GotWeatherForecastListener : IConsumer<IGotWeatherForecast>
    {
        private readonly ICityWeatherForecastService _cityWeatherForecastService;
        private readonly IMapper _mapper;
        private readonly ILogger<GotWeatherForecastListener> _logger;

        public GotWeatherForecastListener(
            ICityWeatherForecastService cityWeatherForecastService,
            IMapper mapper,
            ILogger<GotWeatherForecastListener> logger)
        {
            _cityWeatherForecastService = cityWeatherForecastService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IGotWeatherForecast> context)
        {
            if (context?.Message != null)
            {
                var cityWeatherForecastDocument = _mapper.Map<CityWeatherForecastDocument>(context.Message);
                await _cityWeatherForecastService.CreateAsync(cityWeatherForecastDocument);
            }
            else
            {
                _logger.LogWarning("Received event {EventName} is null.", nameof(IGotWeatherForecast));
            }

            await Task.CompletedTask;
        }
    }
}