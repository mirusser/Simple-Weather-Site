using CitiesService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.Infrastructure.Contexts;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

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