using System.Collections.Generic;

namespace CitiesService.Application.Dto;

public class CitiesPaginationDto
{
    public List<CityDto> Cities { get; set; } = new List<CityDto>();
    public int NumberOfAllCities { get; set; }
}