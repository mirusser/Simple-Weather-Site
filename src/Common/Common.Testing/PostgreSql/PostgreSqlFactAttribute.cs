using System.Runtime.CompilerServices;

namespace Common.Testing.PostgreSql;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (string.IsNullOrWhiteSpace(PostgreSqlTestSettings.GetBaseConnectionString()))
        {
            Skip =
                $"Set env var '{PostgreSqlTestSettings.BaseConnectionStringEnv}' or set '{PostgreSqlTestSettings.BaseConnectionStringKey}' " +
                "in appsettings.local.json to run PostgreSQL integration tests.";
        }
    }
}

public sealed class PostgreSqlTheoryAttribute : TheoryAttribute
{
    public PostgreSqlTheoryAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (string.IsNullOrWhiteSpace(PostgreSqlTestSettings.GetBaseConnectionString()))
        {
            Skip =
                $"Set env var '{PostgreSqlTestSettings.BaseConnectionStringEnv}' or set '{PostgreSqlTestSettings.BaseConnectionStringKey}' " +
                "in appsettings.local.json to run PostgreSQL integration tests.";
        }
    }
}
