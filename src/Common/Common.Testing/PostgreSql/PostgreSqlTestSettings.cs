using Common.Testing.Config;

namespace Common.Testing.PostgreSql;

public static class PostgreSqlTestSettings
{
    public const string BaseConnectionStringEnv = "POSTGRES_BASE_CONNECTION";
    public const string BaseConnectionStringKey = "IntegrationTests:PostgreSqlBaseConnection";

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
