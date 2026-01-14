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

namespace WeatherService.Messages.Queries;

public class GetByCityIdQuery : IRequest<Result<WeatherForecastDto>>
{
    public int CityId { get; set; }
}

// TODO: check publish, add try blocks in other handlers, log errors
public class GetByCityIdHandler(
    WeatherClient weatherClient,
    IPublishEndpoint publishEndpoint,
    IMapper mapper,
    ILogger<GetByCityIdHandler> logger)
    : IRequestHandler<GetByCityIdQuery, Result<WeatherForecastDto>>
{
    public async Task<Result<WeatherForecastDto>> Handle(
        GetByCityIdQuery request,
        CancellationToken cancellationToken)
    {
        var forecastResult = await weatherClient.GetCurrentWeatherByCityIdAsync(request.CityId, cancellationToken);

        if (!forecastResult.IsSuccess)
        {
            return Result<WeatherForecastDto>.Fail(forecastResult.Problem!);
        }

        var weatherForecastDto = mapper.Map<WeatherForecastDto>(forecastResult.Value!);
        var gotWeatherForecastDto = mapper.Map<IGotWeatherForecast>(weatherForecastDto);

        try
        {
            await publishEndpoint.Publish(gotWeatherForecastDto, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while publishing the weather");
        }

        return Result<WeatherForecastDto>.Ok(weatherForecastDto);
    }
}