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

public class GetByCityNameFromXmlResponseQuery : IRequest<Result<WeatherForecastDto>>
{
    public required string City { get; init; }
}

public class GetByCityNameFromXmlResponseHandler(
    WeatherClient weatherClient,
    IPublishEndpoint publishEndpoint,
    IMapper mapper,
    ILogger<GetByCityNameFromXmlResponseHandler> logger)
    : IRequestHandler<GetByCityNameFromXmlResponseQuery, Result<WeatherForecastDto>>
{
    public async Task<Result<WeatherForecastDto>> Handle(
        GetByCityNameFromXmlResponseQuery request,
        CancellationToken cancellationToken)
    {
        var currentResult = await weatherClient.GetCurrentXmlByCityAsync(request.City, cancellationToken);

        if (!currentResult.IsSuccess)
        {
            return Result<WeatherForecastDto>.Fail(currentResult.Problem!);
        }

        var weatherForecastDto = mapper.Map<WeatherForecastDto>(currentResult.Value!);
        var gotWeatherForecastDto = mapper.Map<GotWeatherForecast>(weatherForecastDto);
        
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