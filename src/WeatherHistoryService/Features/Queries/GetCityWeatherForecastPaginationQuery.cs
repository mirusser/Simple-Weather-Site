using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WeatherHistoryService.Models.Dto;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Features.Queries;

public class GetCityWeatherForecastPaginationQuery : IRequest<CityWeatherForecastPaginationDto>
{
	public int NumberOfEntities { get; set; }
	public int PageNumber { get; set; }
}

public class GetCityWeatherForecastPaginationHandler(ICityWeatherForecastService cityWeatherForecastService)
	: IRequestHandler<GetCityWeatherForecastPaginationQuery, CityWeatherForecastPaginationDto>
{
	public async Task<CityWeatherForecastPaginationDto> Handle(
		GetCityWeatherForecastPaginationQuery request,
		CancellationToken cancellationToken)
	{
		var cityWeatherForecastPaginationDto = await cityWeatherForecastService
			.GetCityWeatherForecastPaginationAsync(request.NumberOfEntities, request.PageNumber, cancellationToken);

		return cityWeatherForecastPaginationDto;
	}
}