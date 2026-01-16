using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Mediator;
using Common.Presentation.Http;
using MapsterMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using MQModels.WeatherHistory;
using WeatherService.Clients;
using WeatherService.Models.Dto;

namespace WeatherService.Features.Queries;

public class GetByCityNameQuery : IRequest<Result<WeatherForecastDto>>
{
    public required string City { get; init; }
}

public class GetByCityNameHandler(
    WeatherClient weatherClient,
    IPublishEndpoint publishEndpoint,
    IMapper mapper,
    ILogger<GetByCityNameHandler> logger)
    : IRequestHandler<GetByCityNameQuery, Result<WeatherForecastDto>>
{
    public async Task<Result<WeatherForecastDto>> Handle(
        GetByCityNameQuery request,
        CancellationToken cancellationToken)
    {
        var forecastResult = await weatherClient.GetCurrentWeatherByCityNameAsync(request.City, cancellationToken);

        if (!forecastResult.IsSuccess)
        {
            return Result<WeatherForecastDto>.Fail(forecastResult.Problem!);
        }

        var weatherForecastDto = mapper.Map<WeatherForecastDto>(forecastResult.Value!);
        var gotWeatherForecastDto = mapper.Map<GotWeatherForecast>(weatherForecastDto);

        try
        {
            await publishEndpoint.Publish(gotWeatherForecastDto, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish {Event} for City={City}",
                nameof(GotWeatherForecast),
                request.City);
        }

        return Result<WeatherForecastDto>.Ok(weatherForecastDto);
    }
}