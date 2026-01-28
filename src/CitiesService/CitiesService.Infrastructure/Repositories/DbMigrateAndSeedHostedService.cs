using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Features.City.Services;
using CitiesService.Infrastructure.Contexts;
using Common.Infrastructure.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CitiesService.Infrastructure.Repositories;

public sealed class DbMigrateAndSeedHostedService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<DbMigrateAndSeedHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<ICitiesSeeder>();

        await EnsureDatabaseExistsAsync(cancellationToken);
        
        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation("Applying migrations: {Migrations}", string.Join(", ", pending));
            await db.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Migrations applied.");
        }

        await seeder.SeedIfEmptyAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    
    private async Task EnsureDatabaseExistsAsync(
        CancellationToken ct)
    {
        var cs = configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection))
                 ?? throw new InvalidOperationException($"Missing connection string '{nameof(ConnectionStrings.DefaultConnection)}'.");

        var builder = new SqlConnectionStringBuilder(cs);
        var dbName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(dbName))
        {
            throw new InvalidOperationException("Connection string must include Database/Initial Catalog.");
        }

        // Connect to master to create the target DB if needed
        builder.InitialCatalog = "master";

        await using var conn = new SqlConnection(builder.ConnectionString);
        await conn.OpenAsync(ct);

        var cmdText = 
            """
            IF DB_ID(@db) IS NULL
            BEGIN
                DECLARE @sql nvarchar(max) = N'CREATE DATABASE [' + REPLACE(@db, ']', ']]') + N']';
                EXEC(@sql);
            END
            """;

        await using var cmd = new SqlCommand(cmdText, conn);
        cmd.Parameters.AddWithValue("@db", dbName);

        logger.LogInformation("Ensuring database exists: {Database}", dbName);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
