﻿using System.Collections.Generic;
using CitiesService.Domain.Entities;

namespace CitiesService.Application.Features.City.Models.Dto;

public class CityInfoPaginationDto
{
	public List<CityInfo> CityInfos { get; set; } = [];
    public int NumberOfAllCities { get; set; }
}