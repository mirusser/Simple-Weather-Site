using Microsoft.Extensions.Configuration;

namespace CitiesService.IntegrationTests.Infrastructure.Config;

public static class IntegrationTestConfiguration
{
    public const string SqlServerBaseConnectionKey = "IntegrationTests:SqlServerBaseConnection";

    private static readonly Lazy<IConfigurationRoot> config = new(() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build());

    public static string GetSqlServerBaseConnectionString()
    {
        // Env var stays supported as the highest priority.
        var env = Environment.GetEnvironmentVariable(SqlServer.SqlServerFixture.BaseConnectionStringEnv);
        if (!string.IsNullOrWhiteSpace(env))
        {
            return env;
        }

        return config.Value[SqlServerBaseConnectionKey] ?? string.Empty;
    }
}
