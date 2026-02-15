using CitiesService.IntegrationTests.Infrastructure.Collections;
using CitiesService.IntegrationTests.Infrastructure.Db;
using CitiesService.IntegrationTests.Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;
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

        await db.Database.MigrateAsync();

        var pending = await db.Database.GetPendingMigrationsAsync();
        Assert.Empty(pending);

        // Smoke query: table exists and can be queried
        _ = await db.CityInfos.CountAsync();
    }
}
