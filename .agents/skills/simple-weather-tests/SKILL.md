---
name: simple-weather-tests
description: Use when running, fixing, or documenting Simple Weather Site .NET unit/integration tests, including solution-level dotnet test, CitiesService SQL Server/PostgreSQL resources, test discovery, and shared compiler build quirks.
---

# Simple Weather Tests

Use this workflow for this repo's .NET tests.

## Baseline Commands

Restore first:

```bash
dotnet restore src/SimpleWeather.slnx
```

Run the whole solution with restore disabled and Roslyn shared compilation disabled:

```bash
dotnet test src/SimpleWeather.slnx --no-restore /p:UseSharedCompilation=false /m:1 --verbosity minimal
```

Focused projects:

```bash
dotnet test src/CitiesService/CitiesService.Tests/CitiesService.Tests.csproj --no-restore /p:UseSharedCompilation=false /m:1 --verbosity minimal
dotnet test src/CitiesService/CitiesService.IntegrationTests/CitiesService.IntegrationTests.csproj --no-restore /p:UseSharedCompilation=false /m:1 --verbosity minimal
dotnet test src/IconService/IconService.Test/IconService.Test.csproj --no-restore /p:UseSharedCompilation=false /m:1 --verbosity minimal
dotnet test src/WeatherService/WeatherService.Tests/WeatherService.Tests.csproj --no-restore /p:UseSharedCompilation=false /m:1 --verbosity minimal
```

## Integration Resources

CitiesService integration tests use optional provider-specific connection strings:

- SQL Server: `SQLSERVER_BASE_CONNECTION` or `IntegrationTests:SqlServerBaseConnection`.
- PostgreSQL: `POSTGRES_BASE_CONNECTION` or `IntegrationTests:PostgreSqlBaseConnection`.
- JSON config is loaded from the test output directory using `appsettings.json`, optional gitignored `appsettings.local.json`, then environment variables.
- The example file is `src/CitiesService/CitiesService.IntegrationTests/appsettings.local.json.example`.
- For the repo's local Docker infra, SQL Server is normally reachable from the host on `localhost,1435` and PostgreSQL on `localhost:5432`. Store machine-specific base connections in `src/CitiesService/CitiesService.IntegrationTests/appsettings.local.json`; keep tracked `appsettings.json` blank so provider-specific tests can skip on machines without databases.
- When SQL Server/PostgreSQL integration behavior is the target, do not stop at skipped provider tests. Check env vars, `appsettings.local.json`, running Docker containers, and repo infra docs to infer the base connections; if usable values are found, add them to gitignored `appsettings.local.json`. If the connection cannot be discovered safely, ask the user for it.

The base connection string should target a server/login allowed to create per-test databases and apply EF migrations. Blank values skip that provider's tests. Nonblank but unreachable values fail the run.

Redis and RabbitMQ are not normally required for the current integration suite: API/gRPC factories remove or replace those dependencies where tests need deterministic behavior.

## Verification

The solution-level command should exit `0`. Real test projects should print `Passed!`; when SQL Server and PostgreSQL base connections are configured, `CitiesService.IntegrationTests` should report `Skipped: 0` so provider-specific tests actually ran.

`src/Common/Common.Testing/Common.Testing.csproj` may print `No test is available`; that is acceptable for this helper project when the overall command succeeds.

## Test Discovery Checks

If `dotnet test` builds a project but never prints `Test run for ...`, inspect the project file. Real test projects should include:

```xml
<IsTestProject>true</IsTestProject>
```

`src/Common/Common.Testing/Common.Testing.csproj` is a helper library, not a test suite. Solution-level test runs may report `No test is available` for it; that is acceptable when the command exits successfully.

## Known Quirks

- Use `dotnet restore` before `dotnet test --no-restore`.
- Use `/p:UseSharedCompilation=false /m:1` in this containerized environment to avoid Roslyn shared compiler pipe issues.
- If package-cache or silent build failures appear, use the repo/global `dotnet-build-quirks` skill before retrying tests.
- The Cities API Prometheus test should enable metrics explicitly with `CitiesApiFactory(..., enablePrometheusMetrics: true)`. Keep ordinary API tests on the default factory.
