using Microsoft.Extensions.Configuration;

namespace Common.Testing.Config;

/// <summary>
/// Shared configuration loader for test projects.
///
/// Why: Integration tests often need local configuration (connection strings, ports) without
/// committing secrets to source control. This helper supports:
/// - appsettings.json (committed defaults)
/// - appsettings.local.json (gitignored developer machine overrides)
/// - environment variables (CI or quick overrides)
/// </summary>
public static class TestConfiguration
{
    private static readonly Lazy<IConfigurationRoot> config = new(() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build());

    public static string? Get(string key) => config.Value[key];
}
