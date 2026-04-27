# Simple Weather Site

## Overview

*Simple Weather Site* is a .NET 10 microservices-style application that demonstrates a containerized, service-oriented architecture around weather data. The system includes a Razor Pages UI, REST APIs, gRPC, GraphQL, SignalR, RabbitMQ messaging, background processing, and multiple persistence technologies.

It is built as a practical engineering sandbox for deployment, observability, database migrations, health checks, and integration testing across local Docker, EC2 Docker Compose, and Minikube environments.

![Tests](https://github.com/mirusser/Simple-Weather-Site/actions/workflows/tests.yml/badge.svg?branch=main)
![Build & Run](https://github.com/mirusser/Simple-Weather-Site/actions/workflows/build-run-local.yml/badge.svg?branch=main)

---

## Architecture

### Edge/UI

- **WeatherSite** - Razor Pages web UI. Calls internal APIs, consumes gRPC for city queries, and connects to SignalR for live updates.
- **Gateway (nginx)** - reverse proxy used in EC2 Docker Compose deployment to expose one HTTP entrypoint for WeatherSite and SignalR routes.

### Core services

- **WeatherService** - integrates with OpenWeatherMap and exposes weather HTTP APIs.
- **WeatherHistoryService** - stores and retrieves historical weather results.
- **CitiesService** - HTTP API and GraphQL endpoint for cities. It uses EF Core, runs startup migration/seeding, defaults to PostgreSQL in current appsettings, and still supports SQL Server via provider-specific migrations.
- **CitiesGrpcService** - gRPC service for city-related queries and operations, consumed by WeatherSite.
- **IconService** - stores and serves icon metadata/files with MongoDB-backed persistence and startup seeding.
- **SignalRServer** - hosts SignalR hubs used by the UI for real-time events.
- **EmailService** - internal email/notification service.
- **HangfireService** - background jobs/scheduling and message handling.
- **BackupService** - backup-related tasks, including SQL Server backup support.
- **Common projects** - shared contracts, infrastructure helpers, health checks, mediator implementation, presentation utilities, testing helpers, and cross-service abstractions.

### Authorization

- **OAuthServer** - in-progress identity/token authority for internal clients and services.
- **JwtAuth** - shared auth-related components/utilities, currently a candidate for future cleanup.

### Infrastructure

- **PostgreSQL** - current default database provider for CitiesService.
- **SQL Server** - used by BackupService and still supported by CitiesService.
- **MongoDB** - used by WeatherHistoryService, IconService, Hangfire, and SignalRServer.
- **Redis** - cache.
- **RabbitMQ** - message broker used with MassTransit.
- **Seq** - centralized structured logging target for Serilog.
- **HealthChecks UI** - deployed as an infra container in Docker Compose to aggregate service health endpoints.

---

## Communication

- HTTP REST between WeatherSite and most internal services.
- gRPC between WeatherSite and CitiesGrpcService.
- GraphQL endpoint in CitiesService for city queries.
- SignalR from SignalRServer to the browser.
- RabbitMQ/MassTransit for async and event-driven flows.

---

## Deployment

- **Local Docker Compose:** build images and run the stack from `src/deploy`.
- **AWS EC2 dev deployment:** build locally, push images to ECR, upload compose artifacts, and run on one EC2 VM with nginx as the public entrypoint.
- **Local Kubernetes/Minikube:** build images inside Minikube and apply manifests from `src/deploy/k8s`.

Deployment docs:

- [Docker Compose local and EC2 guide](src/deploy/README.md)
- [Kubernetes/Minikube guide](src/deploy/k8s/README.md)

---

## Health & Observability

- App services expose `/health`, `/health/live`, and `/health/ready`.
- Health responses use `HealthChecks.UI.Client` formatting.
- Docker Compose deployment includes a separate HealthChecks UI container.
- Serilog writes to console; Seq remains as the current legacy structured-log target.
- Docker Compose includes Prometheus/Grafana for metrics, Jaeger for CitiesService traces, and Loki/Grafana logs collected by Alloy for CitiesService containers.
- [Observability run and check guide](AdditionalInfo/Observability.md)

---

## Current State

- Migrated from .NET 5 to .NET 10.
- Service structure reorganized to mimic a microservices system.
- Dockerfiles and Docker Compose deployment exist for the main services.
- Local Docker has an nginx gateway on `http://localhost:8080` so full-site testing can use the same WeatherSite/SignalR routing shape as EC2.
- AWS EC2 dev deployment scripts exist under `src/deploy`.
- Minikube deployment manifests/scripts exist under `src/deploy/k8s`.
- CitiesService database support is provider-selectable, with infrastructure-owned PostgreSQL and SQL Server migration assemblies.
- CitiesService has focused unit and integration test coverage, including PostgreSQL and SQL Server migration/startup workflows.
- CitiesService API and gRPC have the most complete observability path today: Prometheus metrics, Grafana dashboarding, Jaeger traces, Loki logs, trace/log correlation, and local Prometheus rules.

---

## Roadmap Ideas

- CI/CD pipeline improvements.
- Broader test coverage for services outside CitiesService.
- Deeper AWS-native migration, such as EventBridge, SQS, managed databases, and automated backups.
- Export historical weather data as PDF, Excel, CSV, Markdown, or HTML.
- Runtime configuration/admin UI for service URLs and settings.
- User-location based weather lookup.
- More OpenWeather API integrations.
- Evaluate replacing MassTransit when OpenTransit is ready.
- Revisit Duende IdentityServer usage.
- Refresh the UI/frontend, possibly with Blazor.
- Elasticsearch for CitiesService and WeatherSite search.
- Improve security posture and dependency hygiene.
- Explore Semantic Kernel integration for forecast summaries.

### Things to Check Out

- Build useful dashboards and alerts on top of the Prometheus/Grafana metrics stack.
- **Consul** for service discovery and health checking in more advanced microservice setups.
