using System.Collections.Generic;

namespace CitiesService.Application.Features.City.Models.Dto;

public class GetCitiesResult
{
    public IEnumerable<GetCityResult>? Cities { get; set; }
}