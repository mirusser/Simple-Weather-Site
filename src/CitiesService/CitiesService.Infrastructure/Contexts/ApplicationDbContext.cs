using CitiesService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.Infrastructure.Contexts;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<CityInfo> CityInfos { get; set; }

    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     base.OnModelCreating(modelBuilder);
    //
    //     modelBuilder.Entity<CityInfo>()
    //         .HasIndex(c => c.CityId)
    //         .IsUnique();
    // }
}