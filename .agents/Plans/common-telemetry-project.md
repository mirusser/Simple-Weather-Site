# Extract OpenTelemetry to Common.Telemetry Project

## Summary
- Move OpenTelemetry/Serilog setup from `Common.Presentation` to a new `Common.Telemetry` project.
- `Common.Presentation` references the new project and removes moved packages.

## Files to Create

### `src/Common/Common.Telemetry/Common.Telemetry.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.15.3" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.15.3" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.15.2" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.15.1" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.15.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Common.ExtensionMethods\Common.ExtensionMethods.csproj" />
    <ProjectReference Include="..\Common.Shared\Common.Shared.csproj" />
  </ItemGroup>

</Project>
```

### `src/Common/Common.Telemetry/CommonTelemetryRegistration.cs`

```csharp
using System.Reflection;
using Common.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Common.Telemetry;

public static class CommonTelemetryRegistration
{
    private const string ServiceNamespace = "sws";

    public static IServiceCollection AddCommonTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName,
        string environmentName)
    {
        if (!HasOtlpEndpoint(configuration))
        {
            return services;
        }

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                    serviceName: applicationName,
                    serviceNamespace: ServiceNamespace,
                    serviceVersion: GetServiceVersion(),
                    serviceInstanceId: GetServiceInstanceId())
                .AddAttributes([
                    new KeyValuePair<string, object>("deployment.environment.name", environmentName)
                ]))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .UseOtlpExporter();

        return services;
    }

    private static bool HasOtlpEndpoint(IConfiguration configuration)
        => !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"])
           || !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_METRICS_ENDPOINT"]);

    private static string GetServiceVersion()
        => Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
           ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
           ?? "unknown";

    private static string GetServiceInstanceId()
        => $"{Environment.MachineName}:{Environment.ProcessId}";
}
```

## Files to Modify

### `src/Common/Common.Presentation/CommonPresentationRegistration.cs`

**Remove usings:**
- `using System.Reflection;`
- `using OpenTelemetry;`
- `using OpenTelemetry.Metrics;`
- `using OpenTelemetry.Resources;`

**Remove private method** `AddCommonOpenTelemetryMetrics` and its helper methods (`HasOtlpEndpoint`, `GetServiceVersion`, `GetServiceInstanceId`).

**Update `AddCommonPresentationLayer`:**
```csharp
// Before:
builder.Services.AddCommonOpenTelemetryMetrics(...)

// After:
builder.Services.AddCommonTelemetry(
    builder.Configuration,
    builder.Environment.ApplicationName,
    builder.Environment.EnvironmentName);
```

### `src/Common/Common.Presentation/Common.Presentation.csproj`

**Remove package references:**
- `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- `OpenTelemetry.Extensions.Hosting`
- `OpenTelemetry.Instrumentation.AspNetCore`
- `OpenTelemetry.Instrumentation.Http`
- `OpenTelemetry.Instrumentation.Runtime`
- `Serilog.AspNetCore`
- `Serilog.Enrichers.Environment`
- `Serilog.Enrichers.Process`
- `Serilog.Enrichers.Thread`

**Add project reference:**
```xml
<ProjectReference Include="..\Common.Telemetry\Common.Telemetry.csproj" />
```

### `src/SimpleWeather.slnx`

Add to `/Commons/` folder:
```xml
<Project Path="Common/Common.Telemetry/Common.Telemetry.csproj" />
```