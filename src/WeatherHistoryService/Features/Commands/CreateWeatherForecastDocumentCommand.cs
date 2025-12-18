using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Mediator;
using MapsterMapper;
using MassTransit;
using MQModels.WeatherHistory;
using WeatherHistoryService.Models.Dto;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Features.Commands;

public class CreateWeatherForecastDocumentCommand : IRequest<CityWeatherForecastDocument>
{
	public string City { get; set; } = null!;

	public string CountryCode { get; set; } = null!;

	public DateTime SearchDate { get; set; } = DateTime.Now;

	public TemperatureDto Temperature { get; set; } = new();

	public string? Summary { get; set; }

	public string? Icon { get; set; }
}

public class CreateWeatherForecastDocumentHandler(
	ICityWeatherForecastService cityWeatherForecastService,
	IMapper mapper,
	IPublishEndpoint publishEndpoint) : IRequestHandler<CreateWeatherForecastDocumentCommand, CityWeatherForecastDocument>
{
	public async Task<CityWeatherForecastDocument> Handle(
		CreateWeatherForecastDocumentCommand request, 
		CancellationToken cancellationToken)
	{
		var cityWeatherForecastDocument = mapper.Map<CityWeatherForecastDocument>(request);
		var cityWeatherForecast = await cityWeatherForecastService.CreateAsync(cityWeatherForecastDocument);

		await publishEndpoint.Publish<CreatedCityWeatherForecastSearch>(new(), cancellationToken);

		return cityWeatherForecast;
	}
}