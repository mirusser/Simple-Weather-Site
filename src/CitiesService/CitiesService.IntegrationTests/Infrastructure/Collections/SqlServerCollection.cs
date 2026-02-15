using CitiesService.IntegrationTests.Infrastructure.SqlServer;
using Xunit;

namespace CitiesService.IntegrationTests.Infrastructure.Collections;

/// <summary>
/// Collection to share a single SQL Server container across tests.
/// </summary>
[CollectionDefinition(Name)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SqlServer";
}
