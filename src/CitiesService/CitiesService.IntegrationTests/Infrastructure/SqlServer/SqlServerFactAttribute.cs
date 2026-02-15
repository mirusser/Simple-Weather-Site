using CitiesService.IntegrationTests.Infrastructure.Config;
using Xunit;

namespace CitiesService.IntegrationTests.Infrastructure.SqlServer;

/// <summary>
/// Skips tests unless a SQL Server connection string is provided.
/// This avoids hard-failing on machines/CI that don't have the dependency configured.
/// </summary>
public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        var cs = IntegrationTestConfiguration.GetSqlServerBaseConnectionString();
        if (string.IsNullOrWhiteSpace(cs))
        {
            Skip =
                $"Set env var '{SqlServerFixture.BaseConnectionStringEnv}' or set '{IntegrationTestConfiguration.SqlServerBaseConnectionKey}' " +
                "in 'src/CitiesService/CitiesService.IntegrationTests/appsettings.local.json' to run SQL Server integration tests (copy from appsettings.local.json.example).";
        }
    }
}

public sealed class SqlServerTheoryAttribute : TheoryAttribute
{
    public SqlServerTheoryAttribute()
    {
        var cs = IntegrationTestConfiguration.GetSqlServerBaseConnectionString();
        if (string.IsNullOrWhiteSpace(cs))
        {
            Skip =
                $"Set env var '{SqlServerFixture.BaseConnectionStringEnv}' or set '{IntegrationTestConfiguration.SqlServerBaseConnectionKey}' " +
                "in 'src/CitiesService/CitiesService.IntegrationTests/appsettings.local.json' to run SQL Server integration tests (copy from appsettings.local.json.example).";
        }
    }
}
