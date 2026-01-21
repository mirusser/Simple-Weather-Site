using System;
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
        logger.LogInformation("Received {TypeOfMessage} message", nameof(IGotWeatherForecast));

        // TODO: validate message fields?
        var cityWeatherForecastDocument = mapper.Map<CityWeatherForecastDocument>(context.Message);
        await publishEndpoint.Publish(
            new CreatedCityWeatherForecastSearch()
                { EventId = Guid.NewGuid(), GotWeatherForecastEventId = context.Message.EventId },
            context.CancellationToken);

        await cityWeatherForecastService.UpsertIdempotentAsync(cityWeatherForecastDocument, context.CancellationToken);
    }
}