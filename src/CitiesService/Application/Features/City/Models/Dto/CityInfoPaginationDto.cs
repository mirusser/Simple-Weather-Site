using System.Collections.Generic;
using Domain.Entities;

namespace Application.Models.Dto;

public class CityInfoPaginationDto
{
    public List<CityInfo> CityInfos { get; set; } = new List<CityInfo>();
    public int NumberOfAllCities { get; set; }
}