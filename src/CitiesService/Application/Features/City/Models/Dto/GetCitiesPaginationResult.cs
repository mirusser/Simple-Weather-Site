﻿using System.Collections.Generic;

namespace CitiesService.Application.Models.Dto;

public class GetCitiesPaginationResult
{
    public List<GetCityResult> Cities { get; set; } = new List<GetCityResult>();
    public int NumberOfAllCities { get; set; }
}