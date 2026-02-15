using CitiesService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.IntegrationTests.Infrastructure.Db;

public static class DbTestHelpers
{
    public static string CreateDatabaseName(string prefix = "cities_test")
        => $"{prefix}_{Guid.NewGuid():N}";

    public static ApplicationDbContext CreateDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }
}
