using Convey.CQRS.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherService.Models.Dto;

namespace WeatherService.Messages.Queries
{
    public class GetCityByIdQuery : IQuery<WeatherForecastDto>
    {
        public decimal CityId { get; set; }
    }
}
