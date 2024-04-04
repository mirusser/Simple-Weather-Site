using CitiesService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.Infrastructure.Contexts;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : DbContext(options), IApplicationDbContext
{

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.Entity<CityInfo>()
    //        .HasAlternateKey(c => c.CityId)
    //        .HasName("AlternateKey_CityId");

    //    modelBuilder.Entity<CityInfo>()
    //        .HasIndex(c => c.CityId)
    //        .IsUnique();
    //}

    public DbSet<CityInfo> CityInfos { get; set; }
}