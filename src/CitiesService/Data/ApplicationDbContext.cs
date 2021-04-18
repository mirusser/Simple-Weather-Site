using CitiesService.Data.DatabaseModels;
using CitiesService.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesService.Data
{
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
}
