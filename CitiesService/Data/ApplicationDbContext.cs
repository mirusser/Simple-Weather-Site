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
    public class ApplicationDbContext : DbContext
    {
        private readonly ConnectionStrings _connectionStrings;

        public ApplicationDbContext(IOptions<ConnectionStrings> options) : base ()
        {
            _connectionStrings = options.Value;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionStrings.DefaultConnection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CityInfo>()
                .HasAlternateKey(c => c.CityId)
                .HasName("AlternateKey_CityId");

            modelBuilder.Entity<CityInfo>()
                .HasIndex(c => c.CityId)
                .IsUnique();
        }

        public DbSet<CityInfo> CityInfos { get; set; }
    }
}
