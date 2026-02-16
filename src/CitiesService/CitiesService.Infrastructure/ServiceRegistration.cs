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
	extension(IServiceCollection services)
	{
		public IServiceCollection AddInfrastructureLayer(IConfiguration configuration)
		{
			services.Configure<FileUrlsAndPaths>(configuration.GetSection(nameof(FileUrlsAndPaths)));
			services.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));

			// a factory for GraphQL resolvers
			services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
				options.UseSqlServer(
					configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection)),
					b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

			// Scoped DbContext for existing code, created via the factory
			services.AddScoped<ApplicationDbContext>(sp =>
				sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

			// TODO: remove later:
			// services.AddDbContext<ApplicationDbContext>(options =>
			// 	options.UseSqlServer(configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection)),
			// 		b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
			// services.AddTransient<ApplicationDbContext>();

			services.AddCommonInfrastructure(configuration);

			services.AddTransient<IGenericRepository<CityInfo>, GenericRepository<CityInfo>>();
			services.AddHostedService<DbMigrateAndSeedHostedService>();

			return services;
		}
	}
}