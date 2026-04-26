# Observability

This guide shows how to run and check the observability pieces that are currently available in Docker Compose.

The current stack includes:

- health endpoints on application services
- HealthChecks UI container for internal health polling
- Serilog console logs and Seq as the existing structured log target
- OpenTelemetry metrics exported to Prometheus
- Grafana with provisioned Prometheus, Loki, and Jaeger datasources
- Jaeger traces for CitiesService
- Loki logs for the CitiesService container, collected by Grafana Alloy

## Run the stack locally

Start the infrastructure stack first:

```bash
cd <YOUR_REPO>/src/deploy
[ -f .env.infra ] || cp .env.example.infra .env.infra
# Edit .env.infra if you want a real Grafana password.
docker compose \
  --project-name sws-infra \
  --env-file .env.infra \
  -f docker-compose.infra.prod.yml \
  up -d
```

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

Use CitiesService to create request metrics, traces, and logs:

```bash
curl -i http://localhost:8081/health
curl -i http://localhost:8081/health/live
curl -i http://localhost:8081/health/ready
curl -i \
  -H 'Content-Type: application/json' \
  -d '{"numberOfCities":5,"pageNumber":1}' \
  http://localhost:8081/api/City/GetCitiesPagination
```

Wait at least 15 seconds after sending traffic. Metrics are exported on a 15 second interval.

## Health checks

Most HTTP services expose these endpoints:

- `/health`
- `/health/live`
- `/health/ready`

Useful local service URLs:

| Service | Base URL |
| --- | --- |
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
target_info{job=~"sws/CitiesService.Api|CitiesService.Api"}
```

Then check HTTP request metrics:

```promql
http_server_request_duration_seconds_count{job=~"sws/CitiesService.Api|CitiesService.Api"}
```

Prometheus receives application metrics through OTLP HTTP. It does not scrape each application directly, so normal per-app scrape `up` metrics are not expected.

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

## Jaeger

Open:

```text
http://localhost:16686
```

After generating CitiesService traffic:

1. Select service `CitiesService.Api`.
2. Click `Find Traces`.
3. Open a trace and check for HTTP server spans. EF Core or outbound HTTP spans appear when the request path uses those dependencies.

Only CitiesService exports traces for now. Other services currently export metrics only.

## Loki and Alloy

Loki stores the logs that Grafana reads. Check Loki readiness:

```bash
curl -i http://localhost:3100/ready
```

Alloy scrapes Docker logs from the `citiesservice` container and pushes them to Loki. Open Alloy:

```text
http://localhost:12345
```

In Grafana Explore, choose the Loki datasource and run:

```logql
{service_name="citiesservice"}
```

If no logs appear, generate CitiesService traffic again and wait a few seconds:

```bash
curl -i http://localhost:8081/health
```

Seq is still present as the existing Serilog structured logging target. Grafana-visible logs currently come from Loki and Alloy, not from Seq.

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
  admin@<EC2_PUBLIC_IP>
```

Then open:

- Loki readiness: `http://localhost:3100/ready`
- Alloy UI: `http://localhost:12345`

## Troubleshooting

Check container logs:

```bash
docker logs --tail 200 prometheus
docker logs --tail 200 grafana
docker logs --tail 200 jaeger
docker logs --tail 200 loki
docker logs --tail 200 alloy
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
