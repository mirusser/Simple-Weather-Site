using Microsoft.Data.SqlClient;

namespace Common.Testing.SqlServer;

/// <summary>
/// Provides SQL Server connection strings for integration tests.
///
/// The configured value is expected to be a *base* connection string (server/user/pw/etc.)
/// without a specific database. Tests typically create unique databases by setting
/// <see cref="SqlConnectionStringBuilder.InitialCatalog"/> per test.
/// </summary>
public sealed class SqlServerFixture
{
    private readonly string baseConnectionString = SqlServerTestSettings.GetBaseConnectionString();

    public string GetConnectionStringForDatabase(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            throw new InvalidOperationException(
                $"Missing SQL Server base connection string. Set env var '{SqlServerTestSettings.BaseConnectionStringEnv}' " +
                $"or config key '{SqlServerTestSettings.BaseConnectionStringKey}'.");
        }

        var b = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = databaseName
        };
        return b.ConnectionString;
    }
}
