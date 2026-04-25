using Npgsql;

namespace Common.Testing.PostgreSql;

public sealed class PostgreSqlFixture
{
    private readonly string baseConnectionString = PostgreSqlTestSettings.GetBaseConnectionString();

    public string GetConnectionStringForDatabase(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            throw new InvalidOperationException(
                $"Missing PostgreSQL base connection string. Set env var '{PostgreSqlTestSettings.BaseConnectionStringEnv}' " +
                $"or config key '{PostgreSqlTestSettings.BaseConnectionStringKey}'.");
        }

        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = databaseName
        };

        return builder.ConnectionString;
    }
}
