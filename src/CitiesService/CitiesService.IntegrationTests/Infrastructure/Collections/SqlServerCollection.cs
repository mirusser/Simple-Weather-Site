using Common.Testing.SqlServer;
using Xunit;

namespace CitiesService.IntegrationTests.Infrastructure.Collections;

/// <summary>
/// xUnit collection definition (must live in the test assembly) to share a single
/// <see cref="SqlServerFixture"/> across SQL Server integration tests.
/// </summary>
[CollectionDefinition(Name)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SqlServer";
}
