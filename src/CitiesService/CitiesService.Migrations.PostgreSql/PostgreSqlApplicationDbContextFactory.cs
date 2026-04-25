using System;
using System.IO;
using CitiesService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CitiesService.Migrations.PostgreSql;

public sealed class PostgreSqlApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = CreateConfiguration();
        var connectionString = GetConnectionStringOverride(args)
            ?? configuration.GetConnectionString("PostgreSqlConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=CitiesServiceDB;Username=postgres;Password=zaq1@WSX";

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseNpgsql(
            connectionString,
            b => b.MigrationsAssembly(typeof(PostgreSqlApplicationDbContextFactory).Assembly.GetName().Name));

        return new ApplicationDbContext(builder.Options);
    }

    private static IConfiguration CreateConfiguration()
    {
        var basePath = ResolveApiProjectPath();

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static string ResolveApiProjectPath()
    {
        var current = Directory.GetCurrentDirectory();
        var candidates = new[]
        {
            current,
            Path.Combine(current, "..", "CitiesService.Api"),
            Path.Combine(current, "CitiesService.Api"),
            Path.Combine(current, "src", "CitiesService", "CitiesService.Api")
        };

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(Path.Combine(fullPath, "appsettings.json")))
            {
                return fullPath;
            }
        }

        return current;
    }

    private static string? GetConnectionStringOverride(string[] args)
    {
        const string connectionArg = "--connection";
        const string connectionPrefix = "--connection=";

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.Equals(connectionArg, StringComparison.OrdinalIgnoreCase) &&
                i + 1 < args.Length)
            {
                return args[i + 1];
            }

            if (arg.StartsWith(connectionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return arg[connectionPrefix.Length..];
            }
        }

        return null;
    }
}
