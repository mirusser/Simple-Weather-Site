# Observability

This guide shows how to run and check the observability pieces that are currently available in Docker Compose.

The current stack includes:

- health endpoints on application services
- HealthChecks UI container for internal health polling
- Serilog console logs and Seq as the existing structured log target
- OpenTelemetry metrics exported to Prometheus in two ways:
  - `citiesservice` and `citiesgrpcservice` expose `/metrics` for Prometheus scraping
  - the remaining app services push metrics to Prometheus through its OTLP receiver
- Grafana with provisioned Prometheus, Loki, and Jaeger datasources
- Jaeger traces for `CitiesService.Api` and `CitiesGrpcService`, persisted locally with Badger storage
- Loki logs for the CitiesService containers, collected by Grafana Alloy and linked to Jaeger by `trace_id`
- Prometheus recording and alerting rules for the CitiesService observability milestone

## Run the stack locally

Start the infrastructure stack first:

```bash
cd <YOUR_REPO>/src/deploy
./run-infra-locally.sh
```

If `.env.infra` does not exist, the script creates it from `.env.example.infra` and stops. Fill every value in `.env.infra`, then rerun the script.

The script starts `docker-compose.infra.prod.yml`, ensures the shared Docker network and `/opt/sws/volumes/` bind-mount directories exist, and initializes the Mongo replica set required by services that use `mongodb://mongo:27017/?replicaSet=rs0`.

The `.env.infra` file is used for Compose variable substitution. The variables are passed into containers only where `docker-compose.infra.prod.yml` references them. For example, `GRAFANA_ADMIN_PASSWORD` becomes the Grafana container variable `GF_SECURITY_ADMIN_PASSWORD`.

To inspect the Compose interpolation environment:

```bash
cd <YOUR_REPO>/src/deploy
docker compose \
  --project-name sws-infra \
  --env-file .env.infra \
  -f docker-compose.infra.prod.yml \
  config --environment
```

Then render the resolved Compose config:

```bash
cd <YOUR_REPO>/src/deploy
docker compose \
  --project-name sws-infra \
  --env-file .env.infra \
  -f docker-compose.infra.prod.yml \
  config
```

Both commands can print secrets. Do not paste their output publicly if you put real passwords in `.env.infra`.

If Compose reports `Set POSTGRES_PASSWORD in .env.infra`, `Set MSSQL_SA_PASSWORD in .env.infra`, or `Set GRAFANA_ADMIN_PASSWORD in .env.infra`, the variable is missing or empty. Exported shell variables override `.env.infra`, so check your shell environment too if the rendered value is not what you expect.

If the rendered config is correct but services still use old credentials, password state may already be persisted:

- Grafana stores the admin user under `/opt/sws/volumes/grafana/data` after first startup.
- Jaeger stores local trace data under `/opt/sws/volumes/jaeger/badger`.
- Loki stores local log data under `/opt/sws/volumes/loki/data`.
- Prometheus stores local metric data under `/opt/sws/volumes/prometheus/data`.
- PostgreSQL and SQL Server store initialized database state under `/opt/sws/volumes/`.
- HealthChecks UI reads the SQL Server password from Compose each time, but the SQL Server password itself is controlled by the existing SQL Server data.

For a clean local reset, stop the infra stack and remove only the local observability/database data you are willing to lose before starting it again.

Build and start the application containers:

```bash
cd <YOUR_REPO>/src/deploy
PFX_PASSWORD=zaq1@WSX ./build-run-locally.sh
```

Check that the main observability containers are running:

```bash
docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}' \
  | grep -E 'prometheus|grafana|jaeger|loki|alloy|seq|healthchecks|citiesservice'
```

## Generate sample telemetry

Use CitiesService to create REST request metrics, traces, and logs:

```bash
curl -i http://localhost:8081/health
curl -i http://localhost:8081/health/live
curl -i http://localhost:8081/health/ready
curl -i \
  -H 'Content-Type: application/json' \
  -d '{"numberOfCities":5,"pageNumber":1}' \
  http://localhost:8081/api/City/GetCitiesPagination
```

Generate GraphQL traffic:

```bash
curl -i \
  -H 'Content-Type: application/json' \
  -d '{"query":"{ cities(first: 5) { totalCount nodes { id name countryCode } } }"}' \
  http://localhost:8081/graphql
```

From the repository root, generate gRPC traffic:

```bash
grpcurl \
  -plaintext \
  -import-path src/CitiesService/CitiesGrpcService/Protos \
  -proto cities.proto \
  -d '{"numberOfCities":5,"pageNumber":1}' \
  localhost:8682 \
  cities.Cities/GetCitiesPagination
```

Wait for the next Prometheus scrape after sending traffic. The default scrape interval is 15 seconds.

On Debian based Linux distros you may need to install `grpcurl` first, use:

For 64-bit Intel/AMD Debian:
```bash
VERSION=1.9.3
curl -L "https://github.com/fullstorydev/grpcurl/releases/download/v${VERSION}/grpcurl_${VERSION}_linux_x86_64.tar.gz" \
  | sudo tar -xz -C /usr/local/bin grpcurl

grpcurl -version
```

## Health checks

Most HTTP services expose these endpoints:

- `/health`
- `/health/live`
- `/health/ready`

Useful local service URLs:

| Service | Base URL |
| --- | --- |
| **Gateway (nginx)** | `http://localhost:8080` |
| OAuthServer | `http://localhost:8078` |
| CitiesService | `http://localhost:8081` |
| CitiesGrpcService | `http://localhost:8681` |
| WeatherService | `http://localhost:8082` |
| WeatherHistoryService | `http://localhost:8083` |
| WeatherSite | `http://localhost:8084` |
| IconService | `http://localhost:8887` |
| SignalRServer | `http://localhost:8897` |
| EmailService | `http://localhost:8087` |
| HangfireService | `http://localhost:8089` |
| BackupService | `http://localhost:8090` |

Use the gateway URL to open WeatherSite in local Docker:

```text
http://localhost:8080
```

This exercises the same nginx shape as EC2: `/` is proxied to WeatherSite and `/signalr/` is proxied to SignalRServer. SignalR browser connections depend on that route.

Direct service ports are available for debugging individual services. `http://localhost:8084` opens WeatherSite directly and bypasses nginx, so browser requests to `/signalr/...` stay on the WeatherSite container and return `404`.

Example:

```bash
curl -i http://localhost:8082/health
curl -i http://localhost:8084/health/ready
```

The HealthChecks UI container is part of the infrastructure Compose stack and polls internal service URLs. It is not currently published to a localhost browser port, so the user-facing checks are the service health endpoints and container status.

## Prometheus

Open:

```text
http://localhost:9090
```

Use the Prometheus expression page and run:

```promql
up{job=~"citiesservice|citiesgrpcservice"}
```

Then check API and gRPC HTTP request metrics:

```promql
sum by (job) (rate(http_server_request_duration_seconds_count{job=~"citiesservice|citiesgrpcservice"}[5m]))
```

Check custom CitiesService metrics:

```promql
sum by (operation, result) (rate(sws_cities_mediator_requests_total[5m]))
sum by (operation, cache_result) (rate(sws_cities_cache_requests_total[5m]))
histogram_quantile(0.95, sum by (le, operation) (rate(sws_cities_mediator_request_duration_seconds_bucket[5m])))
sum by (grpc_method, result) (rate(sws_cities_grpc_calls_total[5m]))
sum by (grpc_method) (rate(sws_cities_grpc_stream_messages_total[5m]))
```

Prometheus still starts with `--web.enable-otlp-receiver` because the remaining app services push metrics through OTLP HTTP. `citiesservice` and `citiesgrpcservice` intentionally do not set `OTEL_EXPORTER_OTLP_METRICS_ENDPOINT`; Prometheus scrapes their `http://citiesservice:80/metrics` and `http://citiesgrpcservice:80/metrics` endpoints instead. This avoids duplicate API/gRPC metrics.

Prometheus keeps local metrics for up to 15 days or 2 GB, whichever limit is reached first. It also evaluates local-only CitiesService rules from `src/deploy/prometheus/rules/cities-observability.yml`.

Check recording and alerting rule output:

```promql
sws:cities:http_requests:rate5m
sws:cities:http_duration:p95_5m
sws:cities:cache_miss:ratio5m
ALERTS{alertname=~"Cities.*"}
```

The local alert rules do not send notifications because this stack does not include Alertmanager. They are meant to make regressions visible in Prometheus and Grafana.

## Grafana

Open:

```text
http://localhost:3000
```

Sign in with:

- user: `admin`
- password: the `GRAFANA_ADMIN_PASSWORD` value from `src/deploy/.env.infra`

Check these areas:

- `Connections` / `Data sources`: Prometheus, Loki, and Jaeger should be present.
- `Dashboards`: open `Simple Weather Site / CitiesService Observability`.
- `Explore`: choose Prometheus or Loki and run the sample queries from this guide.

The CitiesService dashboard has `service` and `window` variables, panels for HTTP/gRPC latency and errors, cache behavior, firing Cities alerts, observability infra health, and Loki logs. The Loki datasource has a derived field that turns `trace_id` values from JSON logs into Jaeger trace links.

## Jaeger

Open:

```text
http://localhost:16686
```

After generating CitiesService traffic:

1. Select service `CitiesService.Api`.
2. Click `Find Traces`.
3. Open a trace and check for HTTP server spans plus `CitiesService.Application` and `CitiesService.GraphQL` spans when those flows run.
4. Select service `CitiesGrpcService`.
5. Open a trace and check for HTTP/gRPC server spans plus custom `CitiesGrpcService` spans.

REST, GraphQL, and gRPC traces are exported to Jaeger through OTLP HTTP at `http://jaeger:4318/v1/traces`. Other app services currently export metrics only unless their Compose service sets `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT`.

`Common.Telemetry` maps the signal-specific OTLP endpoint/protocol environment variables into `AddOtlpExporter(...)` options explicitly. Keep that mapping in place: the OpenTelemetry .NET signal-specific `AddOtlpExporter()` calls do not read `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT` or `OTEL_EXPORTER_OTLP_METRICS_ENDPOINT` by themselves.

Jaeger uses local Badger storage in this stack. Trace data survives `jaeger_infra` container restarts and is retained for 7 days. To wipe only local Jaeger traces, stop Jaeger and remove `/opt/sws/volumes/jaeger/badger`.

The Cities containers set `SWS_TELEMETRY_TRACES_INCLUDE_INFRA_ENDPOINTS=false`, so ASP.NET Core server traces for `/`, `/ping`, `/metrics`, `/health`, and `/health/*` are filtered out before export. Health and metrics endpoints still work and still produce metrics. Set the variable to `true` or remove it if you want those endpoint traces during a diagnostics session.

If Prometheus/Grafana metrics and Loki logs update but Jaeger shows no traces, verify the trace path directly:

```bash
docker inspect citiesservice \
  --format '{{range .Config.Env}}{{println .}}{{end}}' \
  | grep -E 'OTEL_EXPORTER_OTLP_TRACES_ENDPOINT|OTEL_EXPORTER_OTLP_PROTOCOL|SWS_TELEMETRY'

docker inspect citiesgrpcservice \
  --format '{{range .Config.Env}}{{println .}}{{end}}' \
  | grep -E 'OTEL_EXPORTER_OTLP_TRACES_ENDPOINT|OTEL_EXPORTER_OTLP_PROTOCOL|SWS_TELEMETRY'
```

Both services should include `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT=http://jaeger:4318/v1/traces`. Then check that Jaeger is reachable from the shared Docker network:

```bash
docker run --rm --network sws-containers-bridge-network curlimages/curl:8.11.1 \
  -sS -o /dev/null -w '%{http_code}\n' \
  http://jaeger:4318/v1/traces
```

Any HTTP status means the endpoint is reachable; `405` is expected for this GET-based check because OTLP trace ingestion uses POST. `000` means the app containers cannot connect to Jaeger's OTLP HTTP receiver. Check exporter/receiver logs if connectivity looks good:

```bash
docker logs --tail 200 citiesservice | grep -Ei 'otel|opentelemetry|export|trace|jaeger' || true
docker logs --tail 200 citiesgrpcservice | grep -Ei 'otel|opentelemetry|export|trace|jaeger' || true
docker logs --tail 200 jaeger_infra | grep -Ei 'otlp|trace|span|error|warn' || true
```

You can also query Jaeger's HTTP API instead of the UI:

```bash
curl -s 'http://localhost:16686/api/services'
curl -s 'http://localhost:16686/api/traces?service=CitiesService.Api&lookback=1h&limit=20'
curl -s 'http://localhost:16686/api/traces?service=CitiesGrpcService&lookback=1h&limit=20'
```

For a UI sanity check, try `Last 2 Days` after generating fresh traffic. With Badger storage enabled, recent traces should still be queryable after restarting `jaeger_infra`.

## Loki and Alloy

Loki stores the logs that Grafana reads. Check Loki readiness:

```bash
curl -i http://localhost:3100/ready
```

Alloy scrapes Docker logs from the application containers and pushes them to Loki. Open Alloy:

```text
http://localhost:12345
```

In Grafana Explore, choose the Loki datasource and run:

```logql
{service_name=~"citiesservice|citiesgrpcservice"}
```

Cities Docker logs are JSON formatted. When `SWS_TELEMETRY_LOG_TRACE_CORRELATION_ENABLED=true`, log events written while an `Activity` is current include `trace_id` and `span_id`. These values are kept as log fields/structured metadata, not Loki labels, to avoid high-cardinality label growth.

Useful LogQL checks:

```logql
{service_name=~"citiesservice|citiesgrpcservice"} | json
{service_name=~"citiesservice|citiesgrpcservice"} | json | trace_id != ""
{service_name=~"citiesservice|citiesgrpcservice", level="Error"}
```

In Grafana Explore or the dashboard log panel, expand a log line with `trace_id`; the `Trace` derived field should open the related Jaeger trace.

If no logs appear, generate CitiesService traffic again and wait a few seconds:

```bash
curl -i http://localhost:8081/health
curl -i http://localhost:8681/health
```

Seq is still present as the existing Serilog structured logging target. Grafana-visible logs currently come from Loki and Alloy, not from Seq. Loki keeps local logs for 7 days.

## Telemetry configuration switches

These switches are intentionally runtime-only and do not change emitted metric names, tag names, span names, or dashboard queries:

| Variable | Default | Cities Docker value | Purpose |
| --- | --- | --- | --- |
| `SWS_TELEMETRY_PROMETHEUS_ENDPOINT_ENABLED` | `false` | `true` | Enables the local `/metrics` scrape endpoint. |
| `SWS_TELEMETRY_TRACES_INCLUDE_INFRA_ENDPOINTS` | `true` | `false` | Includes or filters ASP.NET Core server traces for `/`, `/ping`, `/metrics`, `/health`, and `/health/*`. |
| `SWS_TELEMETRY_LOG_TRACE_CORRELATION_ENABLED` | `false` | `true` | Adds `trace_id` and `span_id` to Serilog events when an `Activity` is current. |

## gRPC counters

The dashboard uses custom stable gRPC metrics from `CitiesGrpcService` (`sws_cities_grpc_*`) plus ASP.NET Core HTTP server metrics. Native gRPC counters such as `Grpc.AspNetCore.Server` are EventCounters, so they are useful with `dotnet-counters` for ad-hoc diagnostics but are not exported through OpenTelemetry in this pass:

```bash
dotnet-counters monitor --process-id <PID> --counters Grpc.AspNetCore.Server
```

The alpha OpenTelemetry EventCounters package is intentionally not part of this setup.

## EC2 access

On EC2, Prometheus, Grafana, Jaeger, Loki, and Alloy bind to localhost on the server. Use an SSH tunnel from your machine:

```bash
ssh -i sws-ec2-key-pair.pem \
  -L 9090:localhost:9090 \
  -L 3000:localhost:3000 \
  -L 16686:localhost:16686 \
  admin@<EC2_PUBLIC_IP>
```

Then open:

- Prometheus: `http://localhost:9090`
- Grafana: `http://localhost:3000`
- Jaeger: `http://localhost:16686`

Optional debugging tunnel:

```bash
ssh -i sws-ec2-key-pair.pem \
  -L 3100:localhost:3100 \
  -L 12345:localhost:12345 \
  -L 8888:localhost:8888 \
  admin@<EC2_PUBLIC_IP>
```

Then open:

- Loki readiness: `http://localhost:3100/ready`
- Alloy UI: `http://localhost:12345`
- Jaeger metrics: `http://localhost:8888/metrics`

## Verification checklist

After changing observability config or rebuilding Cities containers:

1. Validate configs:

```bash
cd <YOUR_REPO>/src/deploy
docker compose --project-name sws-infra --env-file .env.infra -f docker-compose.infra.prod.yml config >/dev/null
promtool check config prometheus/prometheus.yml
promtool check rules prometheus/rules/cities-observability.yml
jq empty grafana/provisioning/dashboards/json/citiesservice-observability.json
```

2. Generate REST, GraphQL, and gRPC traffic from the examples above.
3. Prometheus: confirm `up{job=~"citiesservice|citiesgrpcservice"}` is `1`, custom `sws_cities_*` metrics update, and `ALERTS{alertname=~"Cities.*"}` is queryable.
4. Jaeger: confirm `CitiesService.Api` and `CitiesGrpcService` traces appear, then restart `jaeger_infra` and confirm recent traces still query.
5. Loki/Grafana: confirm `{service_name=~"citiesservice|citiesgrpcservice"} | json | trace_id != ""` returns logs and the `Trace` derived field opens Jaeger.

## Reference docs

- Microsoft .NET OpenTelemetry with Prometheus, Grafana, and Jaeger: <https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-prgrja-example>
- .NET custom metrics guidance: <https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-instrumentation>
- .NET custom ActivitySource guidance: <https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs>
- .NET built-in metrics: <https://learn.microsoft.com/en-us/dotnet/core/diagnostics/built-in-metrics>
- EF Core metrics: <https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/metrics>
- gRPC diagnostics and counters: <https://learn.microsoft.com/en-us/aspnet/core/grpc/diagnostics>
- OpenTelemetry .NET exporters: <https://opentelemetry.io/docs/languages/dotnet/exporters/>
- Jaeger 2.17 getting started: <https://www.jaegertracing.io/docs/2.17/getting-started/>
- Jaeger 2.17 configuration: <https://www.jaegertracing.io/docs/2.17/deployment/configuration/>
- Jaeger 2.17 Badger storage: <https://www.jaegertracing.io/docs/2.17/storage/badger/>
- Loki retention: <https://grafana.com/docs/loki/latest/operations/storage/retention/>
- Prometheus alerting rules: <https://prometheus.io/docs/prometheus/latest/configuration/alerting_rules/>

## Troubleshooting

Check container logs:

```bash
docker logs --tail 200 prometheus_infra
docker logs --tail 200 grafana_infra
docker logs --tail 200 jaeger_infra
docker logs --tail 200 loki_infra
docker logs --tail 200 alloy_infra
docker logs --tail 200 citiesservice
```

Restart only the observability containers:

```bash
cd <YOUR_REPO>/src/deploy
docker compose \
  --project-name sws-infra \
  --env-file .env.infra \
  -f docker-compose.infra.prod.yml \
  up -d prometheus grafana jaeger loki alloy
```
