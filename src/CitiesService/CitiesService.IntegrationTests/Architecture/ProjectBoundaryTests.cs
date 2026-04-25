using System.Xml.Linq;
using Xunit;

namespace CitiesService.IntegrationTests.Architecture;

public class ProjectBoundaryTests
{
    private static readonly HashSet<string> EfCoreAllowedProjects =
    [
        "CitiesService.Infrastructure.csproj",
        "CitiesService.Infrastructure.Persistence.csproj",
        "CitiesService.Migrations.PostgreSql.csproj",
        "CitiesService.Migrations.SqlServer.csproj"
    ];

    [Fact]
    public void OnlyInfrastructureFacadeReferencesMigrationProjectsDirectly()
    {
        var violations = LoadCitiesServiceProjects()
            .Select(project => new
            {
                Project = Path.GetFileName(project),
                References = XDocument.Load(project)
                    .Descendants("ProjectReference")
                    .Select(r => r.Attribute("Include")?.Value)
                    .Where(include => include?.Contains("CitiesService.Migrations", StringComparison.OrdinalIgnoreCase) == true)
                    .ToList()
            })
            .Where(x => x.References.Count > 0 && x.Project != "CitiesService.Infrastructure.csproj")
            .Select(x => $"{x.Project}: {string.Join(", ", x.References)}")
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void EfCorePackagesStayInInfrastructureOwnedProjects()
    {
        var violations = LoadCitiesServiceProjects()
            .Select(project => new
            {
                Project = Path.GetFileName(project),
                Packages = XDocument.Load(project)
                    .Descendants("PackageReference")
                    .Select(r => r.Attribute("Include")?.Value)
                    .Where(include => include?.Contains("EntityFrameworkCore", StringComparison.OrdinalIgnoreCase) == true)
                    .ToList()
            })
            .Where(x => x.Packages.Count > 0 && !EfCoreAllowedProjects.Contains(x.Project))
            .Select(x => $"{x.Project}: {string.Join(", ", x.Packages)}")
            .ToList();

        Assert.Empty(violations);
    }

    private static IEnumerable<string> LoadCitiesServiceProjects()
    {
        var root = FindRepoRoot();
        var citiesServiceRoot = Path.Combine(root, "src", "CitiesService");

        return Directory.EnumerateFiles(citiesServiceRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "src", "SimpleWeather.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
