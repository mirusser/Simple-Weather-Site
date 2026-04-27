## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

## 5. Project-Specific Guidelines

### Environment (Antigravity in Docker)

This repo may be worked on by the Antigravity AI agent running in a Docker container with Docker Outside Docker (DooD) enabled. The .NET 10 SDK is installed to `~/.dotnet` (not system-wide). See [`.agents/Plans/environment-setup.md`](.agents/Plans/environment-setup.md) for full details on installed tools and how to reproduce the setup.

### .NET Build Quirks

If `dotnet build` fails silently, reports `NETSDK1064`, or shows Roslyn shared compiler pipe errors, run an explicit restore first and verify with:

```bash
dotnet restore <project>
dotnet msbuild <project> /t:Build /p:Restore=false /p:UseSharedCompilation=false /m:1 /v:minimal
```

Avoid overlapping restore/build operations on the same project. A local Codex skill, `dotnet-build-quirks`, records the fuller recovery checklist.

### Tests Execution

Restore once before test runs, then avoid implicit restore/build overlap:

```bash
dotnet restore src/SimpleWeather.slnx
dotnet test src/SimpleWeather.slnx --no-restore /p:UseSharedCompilation=false /m:1 --verbosity minimal
```

Focused test projects:
- `src/CitiesService/CitiesService.Tests/CitiesService.Tests.csproj` - unit tests.
- `src/CitiesService/CitiesService.IntegrationTests/CitiesService.IntegrationTests.csproj` - API, gRPC, migrations, seeding, startup, and database integration tests.
- `src/IconService/IconService.Test/IconService.Test.csproj` - unit tests.
- `src/WeatherService/WeatherService.Tests/WeatherService.Tests.csproj` - unit tests.

`src/Common/Common.Testing/Common.Testing.csproj` is a shared test helper library. A solution-level `dotnet test` may probe it and print `No test is available`; that is expected unless it is converted into a real test project.

Cities integration tests need database resources only when their base connection strings are configured:
- SQL Server: set `SQLSERVER_BASE_CONNECTION` or `IntegrationTests:SqlServerBaseConnection`.
- PostgreSQL: set `POSTGRES_BASE_CONNECTION` or `IntegrationTests:PostgreSqlBaseConnection`.
- `src/CitiesService/CitiesService.IntegrationTests/appsettings.local.json.example` shows the local JSON shape. Prefer environment variables for one-off runs; use gitignored `appsettings.local.json` for machine defaults.
- For the repo's local Docker infra, SQL Server is normally reachable from the host on `localhost,1435` and PostgreSQL on `localhost:5432`. Put the machine-specific values in `src/CitiesService/CitiesService.IntegrationTests/appsettings.local.json`; keep tracked `appsettings.json` blank so provider-specific tests can skip when databases are unavailable.
- When SQL Server/PostgreSQL integration behavior is the target, do not stop at skipped provider tests. Check env vars, `appsettings.local.json`, running Docker containers, and repo infra docs to infer the base connections; if usable values are found, add them to gitignored `appsettings.local.json`. If the connection cannot be discovered safely, ask the user for it.
- The base connection must point to a server/login that can create per-test databases and apply EF migrations. If the value is blank, provider-specific tests are skipped; if it is set but unreachable, the tests fail.

The Cities API/gRPC integration tests replace Redis/cache and RabbitMQ hosted-service dependencies where needed, so SQL Server/PostgreSQL are the main external resources. The API Prometheus `/metrics` test must opt into metrics with `CitiesApiFactory(..., enablePrometheusMetrics: true)`; keep normal API tests on the default factory so telemetry setup does not leak across test cases.

Verification succeeds when the solution-level command exits `0`, all real test projects print `Passed!`, and the Cities integration project reports `Skipped: 0` when SQL Server and PostgreSQL are configured. `Common.Testing` may still print `No test is available`; that helper-project message is acceptable if the overall command succeeds.

When adding or repairing a test project, make sure the project has `<IsTestProject>true</IsTestProject>` if `dotnet test` only builds it and never prints a `Test run for ...` line.
