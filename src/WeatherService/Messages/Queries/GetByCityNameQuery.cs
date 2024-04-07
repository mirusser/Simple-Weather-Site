using System.Threading;
using System.Threading.Tasks;
using MapsterMapper;
using MassTransit;
using MediatR;
using MQModels.WeatherHistory;
using WeatherService.Clients;
using WeatherService.Models.Dto;

namespace WeatherService.Messages.Queries;

public class GetByCityNameQuery : IRequest<WeatherForecastDto>
{
    public string City { get; set; }
}

public class GetByCityNameHandler : IRequestHandler<GetByCityNameQuery, WeatherForecastDto>
{
    private readonly WeatherClient _weatherClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;

    public GetByCityNameHandler(
        WeatherClient weatherClient,
        IPublishEndpoint publishEndpoint,
        IMapper mapper)
    {
        _weatherClient = weatherClient;
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
    }

    public async Task<WeatherForecastDto> Handle(GetByCityNameQuery request, CancellationToken cancellationToken)
    {
        var forecast = await _weatherClient.GetCurrentWeatherByCityNameAsync(request.City);

        if (forecast == null)
        {
            //TODO: logging
            return new();
        }

        var weatherForecastDto = _mapper.Map<WeatherForecastDto>(forecast);
        var gotWeatherForecast = _mapper.Map<IGotWeatherForecast>(weatherForecastDto);

        await _publishEndpoint.Publish(gotWeatherForecast);

        return weatherForecastDto;
    }
}