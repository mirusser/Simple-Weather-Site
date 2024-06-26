﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MapsterMapper;
using MassTransit;
using MediatR;
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

public class CreateWeatherForecastDocumentHandler : IRequestHandler<CreateWeatherForecastDocumentCommand, CityWeatherForecastDocument>
{
    private readonly ICityWeatherForecastService _cityWeatherForecastService;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateWeatherForecastDocumentHandler(
        ICityWeatherForecastService cityWeatherForecastService,
        IMapper mapper,
        IPublishEndpoint publishEndpoint)
    {
        _cityWeatherForecastService = cityWeatherForecastService;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CityWeatherForecastDocument> Handle(CreateWeatherForecastDocumentCommand request, CancellationToken cancellationToken)
    {
        var cityWeatherForecastDocument = _mapper.Map<CityWeatherForecastDocument>(request);
        var cityWeatherForecast = await _cityWeatherForecastService.CreateAsync(cityWeatherForecastDocument);

        await _publishEndpoint.Publish<CreatedCityWeatherForecastSearch>(new());

        return cityWeatherForecast;
    }
}