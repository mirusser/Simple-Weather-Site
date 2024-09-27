using System.Threading.Tasks;
using MapsterMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using MQModels.WeatherHistory;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Listeners;

public class GotWeatherForecastListener(
	ICityWeatherForecastService cityWeatherForecastService,
	IMapper mapper,
	ILogger<GotWeatherForecastListener> logger,
	IPublishEndpoint publishEndpoint)
	: IConsumer<IGotWeatherForecast>
{
	public async Task Consume(ConsumeContext<IGotWeatherForecast> context)
	{
		if (context?.Message is null)
		{
			logger.LogWarning("Received event {EventName} is null.", nameof(IGotWeatherForecast));
			await Task.CompletedTask;

			return;
		}

		var cityWeatherForecastDocument = mapper.Map<CityWeatherForecastDocument>(context.Message);
		await cityWeatherForecastService.CreateAsync(cityWeatherForecastDocument);

		await publishEndpoint.Publish<CreatedCityWeatherForecastSearch>(new());

		return;
	}
}