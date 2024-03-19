using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

public interface IApplicationDbContext
{
    DbSet<CityInfo> CityInfos { get; set; }
}