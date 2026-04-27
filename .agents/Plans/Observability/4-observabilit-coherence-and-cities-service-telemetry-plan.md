# Observability Coherence + CitiesService Telemetry Plan

## Summary
- Re-check `AdditionalInfo/Observability.md` against the current Compose/Grafana/Jaeger/Loki setup, then update it for the new CitiesService API + gRPC telemetry flow.
- Add a compact `AGENTS.md` note about .NET build quirks observed here, and create a local Codex skill named `dotnet-build-quirks`.
- Expand CitiesService observability using official .NET/OpenTelemetry patterns: built-in meters/traces plus low-cardinality custom metrics/spans for REST/GraphQL/gRPC application flows.
- Add `/metrics` scraping for `citiesservice` and `citiesgrpcservice`; keep OTLP metrics for the other app services.

## Key Changes
- Update `Common.Telemetry` to support optional custom meter/source names, Prometheus scraping exporter, and `/metrics` endpoint mapping. Keep OTLP trace export to Jaeger. Avoid duplicate metrics by switching `citiesservice` and `citiesgrpcservice` from OTLP metrics push to Prometheus scraping.
- Register built-in meters for ASP.NET Core, Kestrel, System.Net, System.Runtime, and EF Core, following Microsoft’s metrics guidance. Keep existing ASP.NET Core, HttpClient, runtime, and EF trace instrumentation.
- Add low-cardinality CitiesService telemetry:
  - Activity sources: `CitiesService.Application`, `CitiesService.GraphQL`, `CitiesGrpcService`.
  - Metrics for mediator request duration/counts, cache hit/miss counts, returned city counts, seeding outcomes/durations, gRPC calls/durations, and streamed message counts.
  - Tags must stay low-cardinality: operation/method name, result/outcome, cache hit/miss, and error type only. Do not tag city names, cache keys, URLs with user data, raw request values, or page numbers on metrics.
- Add `/metrics` only for `citiesservice` and `citiesgrpcservice`. Configure Prometheus scrape jobs for `citiesservice:80/metrics` and `citiesgrpcservice:80/metrics`; keep `--web.enable-otlp-receiver` for the remaining OTLP-push services.
- Add traces for `citiesgrpcservice` by setting `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT=http://jaeger:4318/v1/traces` in local and prod Compose. Do not add the alpha EventCounters OTEL package; document native gRPC counters as `dotnet-counters`-only for now.
- Update Grafana dashboard panels to show both API and gRPC scraped metrics, custom `sws_cities_*` metrics, Loki logs, and Jaeger links/queries for both `CitiesService.Api` and `CitiesGrpcService`.
- Update `AdditionalInfo/Observability.md` with the new split: CitiesService metrics via `/metrics` scrape, other app metrics via OTLP, traces via Jaeger, logs via Loki/Alloy. Add sample REST, GraphQL, and gRPC traffic commands; use `grpcurl -plaintext -import-path src/CitiesService/CitiesGrpcService/Protos -proto cities.proto ... localhost:8682 cities.Cities/GetCitiesPagination`.
- Add `AGENTS.md` build note: if `dotnet build` fails silently or Roslyn shared compiler pipe errors appear, run restore first and verify with `dotnet msbuild <project> /t:Build /p:Restore=false /p:UseSharedCompilation=false /m:1 /v:minimal`; avoid overlapping restore/build on the same project.
- Create local skill at `~/.codex/skills/dotnet-build-quirks/SKILL.md`, using `skill-creator` guidance. It should cover NETSDK1064 restore recovery, Roslyn shared compiler pipe issues, `dotnet msbuild` fallback, build-log diagnostics, Docker socket checks, and when to prefer `dotnet restore` before build.

## Test Plan
- Build checks:
  - `dotnet restore src/CitiesService/CitiesService.Api/CitiesService.Api.csproj`
  - `dotnet msbuild src/Common/Common.Telemetry/Common.Telemetry.csproj /t:Build /p:Restore=false /p:UseSharedCompilation=false /m:1 /v:minimal`
  - `dotnet msbuild src/CitiesService/CitiesService.Api/CitiesService.Api.csproj /t:Build /p:Restore=false /p:UseSharedCompilation=false /m:1 /v:minimal`
  - `dotnet msbuild src/CitiesService/CitiesGrpcService/CitiesGrpcService.csproj /t:Build /p:Restore=false /p:UseSharedCompilation=false /m:1 /v:minimal`
- Unit/integration checks:
  - Add focused tests with `ActivityListener`/`MeterListener` for custom CitiesService telemetry where practical.
  - Extend API/gRPC integration coverage to verify `/metrics` returns Prometheus text after sample traffic.
  - Run existing CitiesService unit and integration tests.
- Compose/runtime checks:
  - `docker compose config` for infra, local, and prod files.
  - `promtool check config` for Prometheus config.
  - Run infra + app locally, hit REST and gRPC sample traffic, then verify Prometheus has `up` for both scrape jobs, `sws_cities_*` metrics, API/gRPC HTTP duration metrics, Jaeger traces for API and gRPC, and Loki logs for `citiesservice`.

## Assumptions
- Scope is `CitiesService.Api` plus `CitiesGrpcService`, not all app services.
- `/metrics` is intentionally added for CitiesService API/gRPC; prod exposure remains Docker-internal unless nginx is explicitly changed later.
- Native gRPC EventCounters are not exported through OpenTelemetry in this pass because the available OTEL EventCounters package is alpha.
- Existing user changes, including moved plan files and the current `AGENTS.md` edit, must be preserved.
- References: Microsoft .NET OpenTelemetry Prometheus/Grafana/Jaeger example, ASP.NET Core metrics docs, .NET ActivitySource guidance, .NET built-in metrics docs, EF Core metrics docs, gRPC diagnostics docs, and OpenTelemetry Prometheus exporter docs.
