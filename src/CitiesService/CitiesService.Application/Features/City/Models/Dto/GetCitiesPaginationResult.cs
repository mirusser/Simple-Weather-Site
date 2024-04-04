using System.Collections.Generic;

namespace CitiesService.Application.Features.City.Models.Dto;

public class GetCitiesPaginationResult
{
    public List<GetCityResult> Cities { get; set; } = [];
    public int NumberOfAllCities { get; set; }
}