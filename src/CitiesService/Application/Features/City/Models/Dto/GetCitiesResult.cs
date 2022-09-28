using System.Collections.Generic;
using CitiesService.Application.Models.Dto;

namespace CitiesService.Application.Features.City.Models.Dto;

public class GetCitiesResult
{
    public IEnumerable<GetCityResult>? Cities { get; set; }
}