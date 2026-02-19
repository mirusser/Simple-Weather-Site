using Xunit;

namespace Common.Testing.SqlServer;

/// <summary>
/// xUnit attribute that skips the test unless SQL Server integration test configuration exists.
/// </summary>
public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(SqlServerTestSettings.GetBaseConnectionString()))
        {
            Skip =
                $"Set env var '{SqlServerTestSettings.BaseConnectionStringEnv}' or set '{SqlServerTestSettings.BaseConnectionStringKey}' " +
                "in appsettings.local.json to run SQL Server integration tests.";
        }
    }
}

/// <summary>
/// xUnit attribute that skips the theory unless SQL Server integration test configuration exists.
/// </summary>
public sealed class SqlServerTheoryAttribute : TheoryAttribute
{
    public SqlServerTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(SqlServerTestSettings.GetBaseConnectionString()))
        {
            Skip =
                $"Set env var '{SqlServerTestSettings.BaseConnectionStringEnv}' or set '{SqlServerTestSettings.BaseConnectionStringKey}' " +
                "in appsettings.local.json to run SQL Server integration tests.";
        }
    }
}
