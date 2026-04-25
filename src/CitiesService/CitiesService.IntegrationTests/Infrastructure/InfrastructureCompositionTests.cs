using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Domain.Entities;
using CitiesService.Infrastructure;
using CitiesService.Infrastructure.Contexts;
using CitiesService.Infrastructure.Database;
using CitiesService.Infrastructure.Repositories;
using Common.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CitiesService.IntegrationTests.Infrastructure;

public class InfrastructureCompositionTests
{
    public static TheoryData<string, string, Type, Type, string, string, string> ProviderCases => new()
    {
        {
            "SqlServer",
            "Server=localhost,1435;Database=cities_composition;User ID=sa;Password=zaq1@WSX;Encrypt=False;TrustServerCertificate=True;",
            typeof(SqlServerDatabaseBootstrapper),
            typeof(SqlServerSeedLockProvider),
            DatabaseMigrationAssemblies.SqlServer,
            "Microsoft.EntityFrameworkCore.SqlServer",
            "20260127160353_Init"
        },
        {
            "PostgreSql",
            "Host=localhost;Port=5432;Database=cities_composition;Username=postgres;Password=zaq1@WSX",
            typeof(PostgreSqlDatabaseBootstrapper),
            typeof(PostgreSqlSeedLockProvider),
            DatabaseMigrationAssemblies.PostgreSql,
            "Npgsql.EntityFrameworkCore.PostgreSQL",
            "20260424161108_InitialPostgreSql"
        }
    };

    [Theory]
    [MemberData(nameof(ProviderCases))]
    public void AddInfrastructureLayer_WiresProviderSpecificDatabaseServicesAndMigrationsAssembly(
        string provider,
        string connectionString,
        Type expectedBootstrapperType,
        Type expectedSeedLockType,
        string expectedMigrationsAssembly,
        string expectedProviderName,
        string expectedMigration)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = provider,
                [$"ConnectionStrings:{nameof(ConnectionStrings.DefaultConnection)}"] = connectionString,
                [$"ConnectionStrings:{nameof(ConnectionStrings.RedisConnection)}"] = "localhost:6379"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddInfrastructureLayer(configuration);

        using var root = services.BuildServiceProvider();
        using var scope = root.CreateScope();

        Assert.IsType(expectedBootstrapperType, scope.ServiceProvider.GetRequiredService<IDatabaseBootstrapper>());
        Assert.IsType(expectedSeedLockType, scope.ServiceProvider.GetRequiredService<ISeedLockProvider>());
        Assert.IsType<CityRepository>(scope.ServiceProvider.GetRequiredService<ICityRepository>());
        Assert.IsType<GenericRepository<CityInfo>>(scope.ServiceProvider.GetRequiredService<IGenericRepository<CityInfo>>());

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var migrations = db.Database.GetMigrations().ToList();
        var migrationsAssembly = db.Database.GetService<IMigrationsAssembly>();

        Assert.Equal(expectedProviderName, db.Database.ProviderName);
        Assert.Equal(expectedMigrationsAssembly, migrationsAssembly.Assembly.GetName().Name);
        Assert.Contains(expectedMigration, migrations);
    }
}
