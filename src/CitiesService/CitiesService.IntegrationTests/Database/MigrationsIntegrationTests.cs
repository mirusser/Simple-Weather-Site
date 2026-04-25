using CitiesService.IntegrationTests.Infrastructure.Collections;
using CitiesService.Infrastructure.Database;
using Common.Infrastructure.Settings;
using Common.Testing.PostgreSql;
using Common.Testing.SqlServer;
using CitiesService.IntegrationTests.Infrastructure.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CitiesService.IntegrationTests.Database;

/// <summary>
/// Verifies EF Core migrations apply successfully against a real SQL Server.
/// This catches provider-specific issues that an in-memory provider won't.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class MigrationsIntegrationTests(SqlServerFixture sql)
{
    [SqlServerFact]
    public async Task CanApplyAllMigrations()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_migrations");
        var cs = sql.GetConnectionStringForDatabase(dbName);

        await using var db = DbTestHelpers.CreateDbContext(cs);

        await DbTestHelpers.MigrateAsync(db);

        var pending = await DbTestHelpers.GetPendingMigrationsAsync(db);
        Assert.Empty(pending);

        // Smoke query: table exists and can be queried
        _ = await DbTestHelpers.CountCitiesAsync(db);
    }
}

[Collection(PostgreSqlCollection.Name)]
public class PostgreSqlMigrationsIntegrationTests(PostgreSqlFixture postgres)
{
    [PostgreSqlFact]
    public async Task CanApplyAllMigrations()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_pg_migrations");
        var cs = postgres.GetConnectionStringForDatabase(dbName);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{nameof(ConnectionStrings.DefaultConnection)}"] = cs,
            })
            .Build();

        var bootstrapper = new PostgreSqlDatabaseBootstrapper(
            config,
            NullLogger<PostgreSqlDatabaseBootstrapper>.Instance);
        await bootstrapper.EnsureDatabaseExistsAsync(CancellationToken.None);

        await using var db = DbTestHelpers.CreatePostgreSqlDbContext(cs);

        await DbTestHelpers.MigrateAsync(db);

        var pending = await DbTestHelpers.GetPendingMigrationsAsync(db);
        Assert.Empty(pending);

        _ = await DbTestHelpers.CountCitiesAsync(db);
    }
}
