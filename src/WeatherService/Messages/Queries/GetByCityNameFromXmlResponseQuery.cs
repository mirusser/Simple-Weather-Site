using System.Threading;
using System.Threading.Tasks;
using Common.Mediator;
using MapsterMapper;
using MassTransit;
using MQModels.WeatherHistory;
using WeatherService.Clients;
using WeatherService.Models.Dto;

namespace WeatherService.Messages.Queries;

public class GetByCityNameFromXmlResponseQuery : IRequest<WeatherForecastDto>
{
    public string City { get; set; }
}

public class GetByCityNameFromXmlResponseHandler : IRequestHandler<GetByCityNameFromXmlResponseQuery, WeatherForecastDto>
{
    private readonly WeatherClient _weatherClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;

    public GetByCityNameFromXmlResponseHandler(
        WeatherClient weatherClient,
        IPublishEndpoint publishEndpoint,
        IMapper mapper)
    {
        _weatherClient = weatherClient;
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
    }

    public async Task<WeatherForecastDto> Handle(GetByCityNameFromXmlResponseQuery request, CancellationToken cancellationToken)
    {
        var current = await _weatherClient.GetCurrentWeatherInXmlByCityNameAsync(request.City);

        if (current == null)
        {
            //TODO: logging
            return new();
        }

        var weatherForecastDto = _mapper.Map<WeatherForecastDto>(current);
        var gotWeatherForecast = _mapper.Map<IGotWeatherForecast>(weatherForecastDto);

        await _publishEndpoint.Publish(gotWeatherForecast);

        return weatherForecastDto;
    }
}