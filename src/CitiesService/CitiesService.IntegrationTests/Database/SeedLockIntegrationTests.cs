using CitiesService.Domain.Entities;
using CitiesService.Infrastructure.Repositories;
using CitiesService.IntegrationTests.Infrastructure.Collections;
using CitiesService.IntegrationTests.Infrastructure.Db;
using CitiesService.IntegrationTests.Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;
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
    public async Task TryAcquireSeedLockAsync_AllowsOnlyOneSession()
    {
        var dbName = DbTestHelpers.CreateDatabaseName("cities_lock");
        var cs = sql.GetConnectionStringForDatabase(dbName);

        await using (var db = DbTestHelpers.CreateDbContext(cs))
        {
            await db.Database.MigrateAsync();
        }

        await using var db1 = DbTestHelpers.CreateDbContext(cs);
        await using var db2 = DbTestHelpers.CreateDbContext(cs);

        var repo1 = new GenericRepository<CityInfo>(db1);
        var repo2 = new GenericRepository<CityInfo>(db2);

        var first = await repo1.TryAcquireSeedLockAsync(CancellationToken.None);
        var second = await repo2.TryAcquireSeedLockAsync(CancellationToken.None);

        Assert.True(first);
        Assert.False(second);
    }
}
