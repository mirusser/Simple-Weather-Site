using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Settings;
using CitiesService.Infrastructure.Contexts;
using CitiesService.Infrastructure.Repositories;
using Common.Infrastructure;
using Common.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CitiesService.Infrastructure;

public static class ServiceRegistration
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<FileUrlsAndPaths>(configuration.GetSection(nameof(FileUrlsAndPaths)));
		services.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));

		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection)),
			b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

		services.AddTransient<ApplicationDbContext>();

		services.AddCommonInfrastructure(configuration);

		#region Repositories

		services.AddTransient<IGenericRepository<CityInfo>, GenericRepository<CityInfo>>();

		#endregion Repositories

		return services;
	}
}