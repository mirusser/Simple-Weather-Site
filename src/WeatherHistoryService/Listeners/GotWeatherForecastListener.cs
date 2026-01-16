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
    : IConsumer<GotWeatherForecast>
{
    public async Task Consume(ConsumeContext<GotWeatherForecast> context)
    {
        // TODO: validate message fields?
        var cityWeatherForecastDocument = mapper.Map<CityWeatherForecastDocument>(context.Message);
        await cityWeatherForecastService.UpsertIdempotentAsync(cityWeatherForecastDocument, context.CancellationToken);

        await publishEndpoint.Publish(new CreatedCityWeatherForecastSearch
        {
            // ideally include something meaningful (e.g., doc id / event id)
        }, context.CancellationToken);

        return;
    }
}