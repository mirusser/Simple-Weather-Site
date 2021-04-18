using CitiesService.Data.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.Data
{
    public interface IApplicationDbContext
    {
        DbSet<CityInfo> CityInfos { get; set; }
    }
}