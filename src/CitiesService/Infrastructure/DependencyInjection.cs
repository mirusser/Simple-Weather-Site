using Application.Common.Interfaces.Persistance;
using Domain.Entities;
using Domain.Settings;
using Infrastructure.Contexts;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
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

        #endregion Caching

        #region Repositories

        services.AddTransient<IGenericRepository<CityInfo>, GenericRepository<CityInfo>>();

        #endregion Repositories

        return services;
    }
}