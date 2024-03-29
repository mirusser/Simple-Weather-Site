﻿//using AutoMapper;
//using Convey.CQRS.Events;
//using Convey.MessageBrokers;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.Json.Serialization;
//using System.Threading.Tasks;
//using WeatherHistoryService.Models.Dto;
//using WeatherHistoryService.Mongo.Documents;
//using WeatherHistoryService.Services.Contracts;

//namespace WeatherHistoryService.Messages.Events.External
//{
//    [Message("serviceexchange")]
//    public class GotWeatherForecastEvent : IEvent
//    {
//        public WeatherForecastDto WeatherForecastDto { get; set; }

//        [JsonConstructor]
//        public GotWeatherForecastEvent(WeatherForecastDto weatherForecastDto)
//        {
//            WeatherForecastDto = weatherForecastDto;
//        }
//    }

//    public class GotWeatherForecastHandler : IEventHandler<GotWeatherForecastEvent>
//    {
//        private readonly ICityWeatherForecastService _cityWeatherForecastService;
//        private readonly IMapper _mapper;
//        private readonly ILogger<GotWeatherForecastHandler> _logger;

//        public GotWeatherForecastHandler(
//            ICityWeatherForecastService cityWeatherForecastService,
//            IMapper mapper,
//            ILogger<GotWeatherForecastHandler> logger)
//        {
//            _cityWeatherForecastService = cityWeatherForecastService;
//            _mapper = mapper;
//            _logger = logger;
//        }

//        public async Task HandleAsync(GotWeatherForecastEvent @event)
//        {
//            if (@event != null && @event.WeatherForecastDto != null)
//            {
//                var cityWeatherForecastDocument = _mapper.Map<CityWeatherForecastDocument>(@event.WeatherForecastDto);
//                await _cityWeatherForecastService.CreateAsync(cityWeatherForecastDocument);
//            }
//            else
//            {
//                _logger.LogWarning($"{System.Reflection.Assembly.GetEntryAssembly().GetName().Name}: Recievied event ({nameof(GotWeatherForecastEvent)}) is null.");
//            }

//            await Task.CompletedTask;
//        }
//    }
//}