using CitiesService.Domain.Entities;
using CitiesService.Infrastructure.Database;
using CitiesService.Infrastructure.Repositories;
using CitiesService.IntegrationTests.Infrastructure.Collections;
using Common.Infrastructure.Settings;
using Common.Testing.PostgreSql;
using Common.Testing.SqlServer;
using CitiesService.IntegrationTests.Infrastructure.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CitiesService.IntegrationTests.Database;

/// <summary>
/// Validates the SQL Server app-lock implementation (sp_getapplock) used to prevent
/// multiple instances seeding the cities table concurrently.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class SeedLockIntegrationTests(SqlServerFixture sql)
{
    [SqlServerFact]
    public async Task SqlServerSeedLock_AllowsOnlyOneSession()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_lock");
        var cs = sql.GetConnectionStringForDatabase(dbName);

        await using (var db = DbTestHelpers.CreateDbContext(cs))
        {
            await DbTestHelpers.MigrateAsync(db);
        }

        await using var db1 = DbTestHelpers.CreateDbContext(cs);
        await using var db2 = DbTestHelpers.CreateDbContext(cs);

        var lock1 = new SqlServerSeedLockProvider(db1);
        var lock2 = new SqlServerSeedLockProvider(db2);

        await using var first = await lock1.TryAcquireAsync("CitiesSeed", CancellationToken.None);
        await using var second = await lock2.TryAcquireAsync("CitiesSeed", CancellationToken.None);

        Assert.NotNull(first);
        Assert.Null(second);
    }
}

[Collection(PostgreSqlCollection.Name)]
public class PostgreSqlSeedLockIntegrationTests(PostgreSqlFixture postgres)
{
    [PostgreSqlFact]
    public async Task PostgreSqlSeedLock_AllowsOnlyOneSession()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_pg_lock");
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

        await using (var db = DbTestHelpers.CreatePostgreSqlDbContext(cs))
        {
            await DbTestHelpers.MigrateAsync(db);
        }

        await using var db1 = DbTestHelpers.CreatePostgreSqlDbContext(cs);
        await using var db2 = DbTestHelpers.CreatePostgreSqlDbContext(cs);

        var lock1 = new PostgreSqlSeedLockProvider(db1);
        var lock2 = new PostgreSqlSeedLockProvider(db2);

        await using var first = await lock1.TryAcquireAsync("CitiesSeed", CancellationToken.None);
        await using var second = await lock2.TryAcquireAsync("CitiesSeed", CancellationToken.None);

        Assert.NotNull(first);
        Assert.Null(second);
    }
}
