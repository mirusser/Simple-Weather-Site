using CitiesService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.Infrastructure.Contexts;

public interface IApplicationDbContext
{
    DbSet<CityInfo> CityInfos { get; set; }
}