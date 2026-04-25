using System;
using Microsoft.Extensions.Configuration;

namespace CitiesService.Infrastructure.Database;

public enum DatabaseProvider
{
    SqlServer,
    PostgreSql
}

public static class DatabaseProviderResolver
{
    public const string ProviderKey = "Database:Provider";
    public const string SqlServerProviderName = "SqlServer";
    public const string PostgreSqlProviderName = "PostgreSql";

    public static DatabaseProvider GetProvider(
        IConfiguration configuration,
        string? providerOverride = null)
    {
        var provider = providerOverride ?? configuration[ProviderKey];
        if (string.IsNullOrWhiteSpace(provider))
        {
            return DatabaseProvider.SqlServer;
        }

        return provider.Trim() switch
        {
            var value when value.Equals(SqlServerProviderName, StringComparison.OrdinalIgnoreCase) =>
                DatabaseProvider.SqlServer,
            var value when value.Equals(PostgreSqlProviderName, StringComparison.OrdinalIgnoreCase) =>
                DatabaseProvider.PostgreSql,
            _ => throw new InvalidOperationException(
                $"Unsupported database provider '{provider}'. Supported values are '{SqlServerProviderName}' and '{PostgreSqlProviderName}'.")
        };
    }
}
