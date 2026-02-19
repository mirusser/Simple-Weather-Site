using Common.Testing.Config;

namespace Common.Testing.SqlServer;

/// <summary>
/// SQL Server integration test settings.
///
/// Why: SQL-backed integration tests are optional on dev machines/CI.
/// We centralize how the base connection string is discovered.
/// </summary>
public static class SqlServerTestSettings
{
    public const string BaseConnectionStringEnv = "SQLSERVER_BASE_CONNECTION";
    public const string BaseConnectionStringKey = "IntegrationTests:SqlServerBaseConnection";

    public static string GetBaseConnectionString()
    {
        var env = Environment.GetEnvironmentVariable(BaseConnectionStringEnv);
        if (!string.IsNullOrWhiteSpace(env))
        {
            return env;
        }

        return TestConfiguration.Get(BaseConnectionStringKey) ?? string.Empty;
    }
}
