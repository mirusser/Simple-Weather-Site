using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Settings;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FileUrlsAndPaths>(configuration.GetSection(nameof(FileUrlsAndPaths)));
            services.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection)),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddTransient<ApplicationDbContext>();

            #region Caching
            services.AddMemoryCache();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetSection(nameof(ConnectionStrings)).GetValue<string>(nameof(ConnectionStrings.RedisConnection));
            });
            #endregion

            #region Repositories
            services.AddTransient<IGenericRepository<CityInfo>, GenericRepository<CityInfo>>();
            #endregion
        }
    }
}
