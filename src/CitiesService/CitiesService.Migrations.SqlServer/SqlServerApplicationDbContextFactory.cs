using System;
using System.IO;
using CitiesService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CitiesService.Migrations.SqlServer;

public sealed class SqlServerApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = CreateConfiguration();
        var connectionString = GetConnectionStringOverride(args)
            ?? configuration.GetConnectionString("SqlServerConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost,1435;Database=CitiesServiceDB;User Id=sa;Password=zaq1@WSX;TrustServerCertificate=True";

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseSqlServer(
            connectionString,
            b => b.MigrationsAssembly(typeof(SqlServerApplicationDbContextFactory).Assembly.GetName().Name));

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
