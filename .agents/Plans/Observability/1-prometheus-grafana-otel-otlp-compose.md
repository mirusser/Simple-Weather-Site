# Minimal Prometheus/Grafana Metrics Via OpenTelemetry OTLP

## First Step
- Save this plan before implementation as `.agents/Plans/prometheus-grafana-otel-otlp-compose.md`.
- Keep that file as the implementation reference, then make the code/config changes.

## Summary
- Start with Docker Compose only.
- Add OpenTelemetry metrics in shared startup code and export metrics to Prometheus via OTLP HTTP.
- Add Prometheus + Grafana to the existing infra compose stack.
- Keep HealthChecks UI and Seq unchanged.

## Key Changes
- In `Common.Presentation`, add shared metrics registration called from `AddCommonPresentationLayer()`:
  - packages: `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Instrumentation.Runtime`
  - collect ASP.NET Core request metrics, outbound HTTP metrics, and runtime metrics
  - set resource labels: `service.name`, `service.namespace=sws`, `service.instance.id`, `service.version`, `deployment.environment.name`
  - export only when `OTEL_EXPORTER_OTLP_ENDPOINT` is configured
- Add Compose infra:
  - `prometheus` with `--web.enable-otlp-receiver`
  - `grafana` with a provisioned Prometheus datasource
  - localhost-only UI ports: `127.0.0.1:9090:9090` and `127.0.0.1:3000:3000`
- Add app env vars in compose:
  - `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf`
  - `OTEL_EXPORTER_OTLP_ENDPOINT=http://prometheus:9090/api/v1/otlp`
  - `OTEL_METRIC_EXPORT_INTERVAL=15000`
- Update `upload-to-ec2.sh` to upload the new Prometheus/Grafana config files.
- Update docs with local access and EC2 SSH-tunnel notes.

## Public Interfaces
- No app `/metrics` endpoint in this first pass.
- New local/dev UI endpoints:
  - Prometheus: `http://localhost:9090`
  - Grafana: `http://localhost:3000`
- Grafana admin password comes from `.env.infra` as `GRAFANA_ADMIN_PASSWORD`.

## Test Plan
- Build `Common.Presentation` and at least `CitiesService.Api`; ideally build all Dockerized apps.
- Validate Prometheus config with `promtool check config` or the Prometheus Docker image.
- Start infra compose with Prometheus/Grafana and required CitiesService dependencies.
- Run `CitiesService.Api` through Docker Compose, hit `http://localhost:8081/health/live`, then wait at least one 15s export interval.
- Query Prometheus for CitiesService.Api telemetry:
  - `target_info{job=~"sws/CitiesService.Api|CitiesService.Api"}`
  - `http_server_request_duration_seconds_count{job=~"sws/CitiesService.Api|CitiesService.Api"}`
- Confirm existing `/health`, `/health/live`, and `/health/ready` behavior is unchanged.

## Assumptions
- Docker Compose means local/EC2 compose first; Minikube observability is a follow-up.
- Metrics only for v1; no trace/log export yet.
- OTLP push means Prometheus will not provide normal per-app scrape `up` metrics; HealthChecks UI remains the availability view.
- References: Microsoft ASP.NET Core metrics docs, Prometheus OTLP backend guide, OpenTelemetry NuGet docs, and Grafana provisioning docs.
