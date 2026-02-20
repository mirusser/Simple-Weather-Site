using System.Runtime.CompilerServices;

namespace Common.Testing.SqlServer;

/// <summary>
/// xUnit attribute that skips the test unless SQL Server integration test configuration exists.
/// </summary>
public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1) 
        : base(sourceFilePath, sourceLineNumber)
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
    public SqlServerTheoryAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1) 
        : base(sourceFilePath, sourceLineNumber)
    {
        if (string.IsNullOrWhiteSpace(SqlServerTestSettings.GetBaseConnectionString()))
        {
            Skip =
                $"Set env var '{SqlServerTestSettings.BaseConnectionStringEnv}' or set '{SqlServerTestSettings.BaseConnectionStringKey}' " +
                "in appsettings.local.json to run SQL Server integration tests.";
        }
    }
}
