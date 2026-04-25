using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Infrastructure.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CitiesService.Infrastructure.Database;

public sealed class SqlServerDatabaseBootstrapper(
    IConfiguration configuration,
    ILogger<SqlServerDatabaseBootstrapper> logger) : IDatabaseBootstrapper
{
    public async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        var cs = configuration.GetConnectionString(nameof(ConnectionStrings.DefaultConnection))
                 ?? throw new InvalidOperationException($"Missing connection string '{nameof(ConnectionStrings.DefaultConnection)}'.");

        var builder = new SqlConnectionStringBuilder(cs);
        var dbName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(dbName))
        {
            throw new InvalidOperationException("Connection string must include Database/Initial Catalog.");
        }

        builder.InitialCatalog = "master";

        await using var conn = new SqlConnection(builder.ConnectionString);
        await conn.OpenAsync(cancellationToken);

        const string cmdText =
            """
            IF DB_ID(@db) IS NULL
            BEGIN
                DECLARE @sql nvarchar(max) = N'CREATE DATABASE [' + REPLACE(@db, ']', ']]') + N']';
                EXEC(@sql);
            END
            """;

        await using var cmd = new SqlCommand(cmdText, conn);
        cmd.Parameters.AddWithValue("@db", dbName);

        logger.LogInformation("Ensuring SQL Server database exists: {Database}", dbName);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
