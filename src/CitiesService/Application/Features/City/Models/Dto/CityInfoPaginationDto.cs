using System.Collections.Generic;
using CitiesService.Domain.Entities;

namespace CitiesService.Application.Models.Dto;

public class CityInfoPaginationDto
{
    public List<CityInfo> CityInfos { get; set; } = new List<CityInfo>();
    public int NumberOfAllCities { get; set; }
}