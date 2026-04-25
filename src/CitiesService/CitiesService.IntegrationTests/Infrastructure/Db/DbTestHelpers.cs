using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Infrastructure.Contexts;
using CitiesService.Infrastructure.Database;
using CitiesService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CitiesService.IntegrationTests.Infrastructure.Db;

public static class DbTestHelpers
{
    public static string CreateDatabaseName(string prefix = "cities_test")
        => $"{prefix}_{Guid.NewGuid():N}";

    public static ApplicationDbContext CreateDbContext(string connectionString)
        => CreateSqlServerDbContext(connectionString);

    public static ApplicationDbContext CreateSqlServerDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly(DatabaseMigrationAssemblies.SqlServer))
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }

    public static ApplicationDbContext CreatePostgreSqlDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(
                connectionString,
                b => b.MigrationsAssembly(DatabaseMigrationAssemblies.PostgreSql))
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }

    public static void AddSqlServerApplicationDbContext(
        IServiceCollection services,
        string connectionString)
        => services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly(DatabaseMigrationAssemblies.SqlServer)));

    public static void AddPostgreSqlApplicationDbContext(
        IServiceCollection services,
        string connectionString)
        => services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                b => b.MigrationsAssembly(DatabaseMigrationAssemblies.PostgreSql)));

    public static void ReplaceWithSqlServerApplicationDbContext(
        IServiceCollection services,
        string connectionString)
    {
        services.RemoveAll<ApplicationDbContext>();
        services.RemoveAll<IDbContextFactory<ApplicationDbContext>>();
        services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
        services.RemoveAll<IDatabaseBootstrapper>();
        services.RemoveAll<ISeedLockProvider>();

        services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly(DatabaseMigrationAssemblies.SqlServer)));
        services.AddScoped<ApplicationDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());
        services.AddScoped<IDatabaseBootstrapper, SqlServerDatabaseBootstrapper>();
        services.AddScoped<ISeedLockProvider, SqlServerSeedLockProvider>();
    }

    public static async Task MigrateAsync(ApplicationDbContext db)
        => await db.Database.MigrateAsync();

    public static async Task<IReadOnlyList<string>> GetPendingMigrationsAsync(ApplicationDbContext db)
        => (await db.Database.GetPendingMigrationsAsync()).ToList();

    public static async Task<int> CountCitiesAsync(ApplicationDbContext db)
        => await db.CityInfos.CountAsync();

    public static async Task<bool> AnyCitiesAsync(ApplicationDbContext db)
        => await db.CityInfos.AnyAsync();
}
