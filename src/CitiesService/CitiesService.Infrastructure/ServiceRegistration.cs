using System;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Settings;
using CitiesService.Infrastructure.Contexts;
using CitiesService.Infrastructure.Database;
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

			var provider = DatabaseProviderResolver.GetProvider(configuration);
			var connectionString = configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection))
				?? throw new InvalidOperationException($"Missing connection string '{nameof(ConnectionStrings.DefaultConnection)}'.");

			// a factory for GraphQL resolvers
			services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
				ConfigureDbContext(options, provider, connectionString));

			// Scoped DbContext for existing code, created via the factory
			services.AddScoped<ApplicationDbContext>(sp =>
				sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

			services.AddCommonInfrastructure(configuration);

			services.AddTransient<IGenericRepository<CityInfo>, GenericRepository<CityInfo>>();
			services.AddTransient<ICityRepository, CityRepository>();
			switch (provider)
			{
				case DatabaseProvider.SqlServer:
					services.AddScoped<IDatabaseBootstrapper, SqlServerDatabaseBootstrapper>();
					services.AddScoped<ISeedLockProvider, SqlServerSeedLockProvider>();
					break;
				case DatabaseProvider.PostgreSql:
					services.AddScoped<IDatabaseBootstrapper, PostgreSqlDatabaseBootstrapper>();
					services.AddScoped<ISeedLockProvider, PostgreSqlSeedLockProvider>();
					break;
				default:
					throw new InvalidOperationException($"Unsupported database provider '{provider}'.");
			}
			services.AddHostedService<DbMigrateAndSeedHostedService>();

			return services;
		}
	}

	private static void ConfigureDbContext(
		DbContextOptionsBuilder options,
		DatabaseProvider provider,
		string connectionString)
	{
		switch (provider)
		{
			case DatabaseProvider.SqlServer:
				options.UseSqlServer(
					connectionString,
					b => b.MigrationsAssembly(DatabaseMigrationAssemblies.SqlServer));
				break;
			case DatabaseProvider.PostgreSql:
				options.UseNpgsql(
					connectionString,
					b => b.MigrationsAssembly(DatabaseMigrationAssemblies.PostgreSql));
				break;
			default:
				throw new InvalidOperationException($"Unsupported database provider '{provider}'.");
		}
	}
}
