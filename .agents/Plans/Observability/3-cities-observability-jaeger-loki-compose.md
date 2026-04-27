# CitiesService Observability: Jaeger Traces, Loki Logs, Grafana Dashboard

## Summary
- Save this implementation plan first as `.agents/Plans/cities-observability-jaeger-loki-compose.md`.
- Extend the existing Compose observability stack with Jaeger for traces, Loki + Grafana Alloy for Docker log scraping, and Grafana provisioning for Prometheus, Loki, and Jaeger.
- Keep the app changes scoped to shared telemetry plus CitiesService trace enablement; no public API changes.

## Key Changes
- Refactor `Common.Telemetry` to use signal-specific OTLP export:
  - Metrics export remains enabled when `OTEL_EXPORTER_OTLP_ENDPOINT` or `OTEL_EXPORTER_OTLP_METRICS_ENDPOINT` is configured.
  - Trace export is enabled only when `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT` is configured.
  - Replace global `.UseOtlpExporter()` with signal-specific OTLP exporters so metrics can go to Prometheus and traces to Jaeger.
  - Add tracing instrumentation for ASP.NET Core, HttpClient, and EF Core; use `OpenTelemetry.Instrumentation.EntityFrameworkCore` `1.15.0-beta.1` with default options and no SQL text capture.
- Update Compose app env vars:
  - Replace app-wide `OTEL_EXPORTER_OTLP_ENDPOINT=http://prometheus:9090/api/v1/otlp` with `OTEL_EXPORTER_OTLP_METRICS_ENDPOINT=http://prometheus:9090/api/v1/otlp/v1/metrics`.
  - Keep `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf` and `OTEL_METRIC_EXPORT_INTERVAL=15000`.
  - Add only to `citiesservice`: `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT=http://jaeger:4318/v1/traces`.
- Add Compose infra:
  - `jaeger` using `cr.jaegertracing.io/jaegertracing/jaeger:2.17.0`, localhost UI `127.0.0.1:16686:16686`, internal OTLP HTTP `4318`.
  - `loki` using `grafana/loki:3.7.0`, with a repo-owned local config and `loki-data` volume.
  - `alloy` using `grafana/alloy:v1.15.0`, mounted to `/var/run/docker.sock:ro`, configured to scrape only the `citiesservice` container logs and push them to Loki.
- Update Grafana provisioning:
  - Keep Prometheus datasource UID `sws-prometheus`.
  - Add Loki datasource UID `sws-loki`.
  - Add Jaeger datasource UID `sws-jaeger`.
  - Add one CitiesService dashboard with Prometheus request panels and Loki log panels filtered to `service_name="citiesservice"`.
- Update deployment/docs:
  - Add `loki` and `alloy` config directories to `upload-to-ec2.sh`.
  - Document local/EC2 access for Grafana, Prometheus, Jaeger, Loki readiness, and Alloy UI.
  - Update SSH tunnel examples to include Jaeger `16686`; Loki/Alloy ports are optional debugging endpoints.

## Public Interfaces
- No app HTTP, GraphQL, gRPC, or health endpoint changes.
- New local/EC2 observability endpoints:
  - Jaeger UI: `http://localhost:16686`
  - Loki readiness/debug: `http://localhost:3100/ready`
  - Alloy UI: `http://localhost:12345`
  - Grafana continues at `http://localhost:3000`

## Test Plan
- Build checks:
  - `dotnet build src/Common/Common.Telemetry/Common.Telemetry.csproj`
  - `dotnet build src/CitiesService/CitiesService.Api/CitiesService.Api.csproj`
- Compose validation:
  - Validate `docker-compose.infra.prod.yml`, `docker-compose.local.yml`, and `docker-compose.prod.yml` with `docker compose config`.
- Runtime verification:
  - Start infra Compose, then app Compose.
  - Hit CitiesService: `POST http://localhost:8081/api/City/GetCitiesPagination` with `{"numberOfCities":5,"pageNumber":1}`.
  - Prometheus: confirm `target_info` and `http_server_request_duration_seconds_count` still show CitiesService metrics.
  - Jaeger: confirm service `CitiesService.Api` appears and traces include HTTP server spans plus EF/HTTP dependency spans where applicable.
  - Loki: query `{service_name="citiesservice"}` and confirm CitiesService console logs appear.
  - Grafana: confirm Prometheus, Loki, and Jaeger datasources are provisioned and the CitiesService dashboard loads.

## Assumptions
- User choices locked this plan to all three signals, Loki scrape, and Docker Compose only.
- Logs are collected from existing Serilog console output via Alloy; no Serilog Loki sink and no OpenTelemetry log exporter in this pass.
- Traces are enabled only for `CitiesService.Api`; other services keep metrics-only behavior.
- Jaeger uses transient all-in-one storage for this dev/demo milestone.
- References: Jaeger 2.17 getting started, OpenTelemetry OTLP endpoint config, OpenTelemetry ASP.NET Core traces, Grafana Loki Docker install, Grafana Alloy Docker log source, and Grafana datasource provisioning.
