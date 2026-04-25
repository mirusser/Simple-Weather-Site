using Xunit;

namespace CitiesService.IntegrationTests.Architecture;

public class RuntimeMigrationAssemblyTests
{
    [Fact]
    public void ApiOutputContainsProviderMigrationAssembliesThroughInfrastructureReference()
    {
        AssertMigrationAssembliesExistNextTo(typeof(CitiesService.Api.Controllers.CityController).Assembly.Location);
    }

    [Fact]
    public void GrpcOutputContainsProviderMigrationAssembliesThroughInfrastructureReference()
    {
        AssertMigrationAssembliesExistNextTo(typeof(CitiesGrpcService.Services.CitiesService).Assembly.Location);
    }

    private static void AssertMigrationAssembliesExistNextTo(string assemblyLocation)
    {
        var outputDirectory = Path.GetDirectoryName(assemblyLocation)
            ?? throw new InvalidOperationException($"Could not resolve output directory for '{assemblyLocation}'.");

        Assert.True(
            File.Exists(Path.Combine(outputDirectory, "CitiesService.Migrations.SqlServer.dll")),
            $"Missing SQL Server migrations assembly in '{outputDirectory}'.");
        Assert.True(
            File.Exists(Path.Combine(outputDirectory, "CitiesService.Migrations.PostgreSql.dll")),
            $"Missing PostgreSQL migrations assembly in '{outputDirectory}'.");
    }
}
