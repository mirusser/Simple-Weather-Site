using Common.Testing.PostgreSql;
using Xunit;

namespace CitiesService.IntegrationTests.Infrastructure.Collections;

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSql";
}
