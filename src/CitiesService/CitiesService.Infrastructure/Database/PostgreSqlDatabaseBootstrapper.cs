using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CitiesService.Infrastructure.Database;

public sealed class PostgreSqlDatabaseBootstrapper(
    IConfiguration configuration,
    ILogger<PostgreSqlDatabaseBootstrapper> logger) : IDatabaseBootstrapper
{
    private const string DuplicateDatabaseSqlState = "42P04";

    public async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        var cs = configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection))
                 ?? throw new InvalidOperationException($"Missing connection string '{nameof(ConnectionStrings.DefaultConnection)}'.");

        var builder = new NpgsqlConnectionStringBuilder(cs);
        var dbName = builder.Database;

        if (string.IsNullOrWhiteSpace(dbName))
        {
            throw new InvalidOperationException("Connection string must include Database.");
        }

        builder.Database = "postgres";

        await using var conn = new NpgsqlConnection(builder.ConnectionString);
        await conn.OpenAsync(cancellationToken);

        await using (var existsCmd = new NpgsqlCommand(
                         "SELECT 1 FROM pg_database WHERE datname = @db",
                         conn))
        {
            existsCmd.Parameters.AddWithValue("db", dbName);
            var exists = await existsCmd.ExecuteScalarAsync(cancellationToken);
            if (exists is not null)
            {
                logger.LogInformation("PostgreSQL database already exists: {Database}", dbName);
                return;
            }
        }

        var quotedDbName = QuoteIdentifier(dbName);
        await using var createCmd = new NpgsqlCommand($"CREATE DATABASE {quotedDbName}", conn);

        try
        {
            logger.LogInformation("Creating PostgreSQL database: {Database}", dbName);
            await createCmd.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == DuplicateDatabaseSqlState)
        {
            logger.LogInformation("PostgreSQL database was created by another instance: {Database}", dbName);
        }
    }

    private static string QuoteIdentifier(string identifier)
        => "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}
