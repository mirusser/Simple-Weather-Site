using System;
using Microsoft.Data.SqlClient;
using CitiesService.IntegrationTests.Infrastructure.Config;

namespace CitiesService.IntegrationTests.Infrastructure.SqlServer;

/// <summary>
/// Provides a real SQL Server connection string for integration tests.
///
/// These tests intentionally don't spin up Docker containers (no Testcontainers dependency).
/// To run them, set the environment variable:
/// - CITIES_IT_SQLSERVER_BASE_CONNECTION
/// Example:
///   Server=localhost,1433;User ID=sa;Password=Your_password123;Encrypt=False;TrustServerCertificate=True;
///
/// The fixture will create unique databases by setting Initial Catalog per test.
/// </summary>
public sealed class SqlServerFixture
{
    public const string BaseConnectionStringEnv = "CITIES_IT_SQLSERVER_BASE_CONNECTION";

    private readonly string baseConnectionString;

    public SqlServerFixture()
    {
        baseConnectionString = IntegrationTestConfiguration.GetSqlServerBaseConnectionString();
    }

    public string GetConnectionStringForDatabase(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            throw new InvalidOperationException(
                $"Missing env var '{BaseConnectionStringEnv}'. Set it to run SQL Server integration tests.");
        }

        var b = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = databaseName
        };
        return b.ConnectionString;
    }
}
