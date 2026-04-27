# Cities Observability Polish

## Summary
- Keep scope to `CitiesService.Api`, `CitiesGrpcService`, and local observability infra. Do not roll tracing/metrics/log changes into other app services yet.
- Make Jaeger persistent for dev/demo with local Badger storage and 7-day trace retention.
- Improve usefulness of existing signals: configurable trace-noise filtering, trace/log correlation, better Grafana panels, Prometheus rules, and an end-to-end smoke checklist.

## Public Config And Interfaces
- Add `SWS_TELEMETRY_TRACES_INCLUDE_INFRA_ENDPOINTS`, default `true`. Set it to `false` only for `citiesservice` and `citiesgrpcservice` in local/prod Compose so `/`, `/ping`, `/metrics`, `/health`, and `/health/*` stop cluttering Jaeger while metrics still collect normally.
- Add `SWS_TELEMETRY_LOG_TRACE_CORRELATION_ENABLED`, default `false`. Set it to `true` only for Cities containers so Serilog emits `trace_id` and `span_id` from `Activity.Current`.
- Add Jaeger metrics on localhost port `8888`; Prometheus rule evaluation only, with no Alertmanager container or notifications.

## Key Changes
- Add `src/deploy/jaeger/config-badger.yaml`; replace Jaeger’s current `--set` args with `--config=/etc/jaeger/config.yaml`; mount `/opt/sws/volumes/jaeger/badger`; configure OTLP `4317/4318`, UI/API `16686`, metrics `8888`, Badger `ephemeral: false`, and `ttl.spans: 168h`. Add the Jaeger volume dir to `run-infra-locally.sh`.
- Update `Common.Telemetry` tracing setup with the config-driven ASP.NET Core trace filter. Do not change metric names, tag names, custom spans, or dashboard PromQL schema.
- Add optional Serilog activity enrichment in `Common.Presentation`; normalize Cities API/gRPC Serilog config so Docker console logs are structured JSON and Seq still receives structured events.
- Update Alloy to collect both Cities containers, parse JSON logs, label only low-cardinality fields like `service_name`, `container`, and `level`, and keep `trace_id`/`span_id` as fields rather than Loki labels.
- Add Loki datasource derived-field linking from `trace_id` to Jaeger; update the Cities dashboard with service/window variables, error-rate panels, p95 panels, cache miss ratio, firing-alert state, infra health, and trace-linked logs.
- Add `prometheus/rules/cities-observability.yml` with recording/alerting rules for scrape down, HTTP 5xx, high p95 latency, gRPC exceptions, and high cache miss ratio. Configure Prometheus `rule_files`, scrape Jaeger/Loki/Alloy/Grafana metrics, set Prometheus retention to `15d`/`2GB`, and set Loki retention to `7d` via compactor.
- Update `AdditionalInfo/Observability.md` with retention/reset notes, new config keys, trace/log correlation queries, Prometheus alert checks, and a success checklist.

## Test Plan
- Run focused builds/tests for `Common.Presentation`, `Common.Telemetry`, `CitiesService.Api`, `CitiesGrpcService`, and Cities unit tests.
- Validate infra config with `docker compose config`, `promtool check config`, `promtool check rules`, and `jq` for Grafana JSON.
- Runtime smoke: start infra/apps, generate REST/GraphQL/gRPC traffic, verify Prometheus metrics and `ALERTS`, verify Jaeger traces excluding infra endpoints, restart `jaeger_infra` and confirm traces survive, verify Loki has both Cities containers with `trace_id`/`span_id`, and verify Grafana log-to-Jaeger links.
- Finish with `git diff --check`.

## Assumptions
- Rule thresholds use dev/demo defaults: scrape down `2m`, HTTP 5xx `> 0` for `5m`, p95 latency `> 1s` for `5m`, gRPC exceptions `> 0` for `5m`, cache miss ratio `> 80%` for `10m`.
- Health and metrics endpoint metrics remain visible even when trace filtering is enabled.
- References: [Jaeger 2.17 configuration](https://www.jaegertracing.io/docs/2.17/deployment/configuration/), [Jaeger Badger storage](https://www.jaegertracing.io/docs/2.17/storage/badger/), [OpenTelemetry .NET log correlation](https://opentelemetry.io/docs/languages/dotnet/logs/correlation/), [Grafana Alloy Docker logs](https://grafana.com/docs/alloy/latest/reference/components/loki/loki.source.docker/), [Loki retention](https://grafana.com/docs/loki/latest/operations/storage/retention/), [Prometheus alerting rules](https://prometheus.io/docs/prometheus/3.5/configuration/alerting_rules/).
