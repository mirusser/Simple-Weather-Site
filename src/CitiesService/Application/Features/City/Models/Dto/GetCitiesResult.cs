using System.Collections.Generic;
using Application.Models.Dto;

namespace Application.Features.City.Models.Dto;

public class GetCitiesResult
{
    public IEnumerable<GetCityResult>? Cities { get; set; }
}