using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Contexts
{
    public interface IApplicationDbContext
    {
        DbSet<CityInfo> CityInfos { get; set; }
    }
}