# Simple Weather Site

## Overview

*Simple Weather Site* is a .NET 10 microservices-style application that provides a web UI for checking weather, browsing/searching cities, and viewing historical results (more features in the future). The system is designed to run locally via Docker Compose and can be deployed in a “single VM” dev setup on AWS (EC2 + Docker Compose), pulling images from AWS ECR.

**Project goal:** This is primarily a learning project that aims to mimic a small but feature-rich production-style system (microservices, async messaging, background jobs, health checks, observability, containerized deployment etc.).

---

### High-level architecture

The solution consists of:

### Edge/UI

- **WeatherSite** – Razor Pages web UI (end-user entry point). Calls internal APIs, consumes gRPC for city queries, and connects to SignalR for live updates.

- **Gateway (nginx)** – reverse proxy used in EC2 deployment to expose a single HTTP entrypoint and route traffic to internal containers (WeatherSite + SignalR hub).

### Core services

- **WeatherService** – integrates with external weather provider (OpenWeatherMap) and exposes an HTTP API consumed by WeatherSite.

- **WeatherHistoryService** – stores/retrieves historical weather results and exposes an HTTP API.

- **CitiesService** – HTTP API for cities; uses SQL Server and runs EF Core migrations/seeding (optionally at startup).

- **CitiesGrpcService** – gRPC service for city-related queries/operations (consumed by WeatherSite).

- **IconService** – stores/serves icon metadata/files; uses MongoDB and may seed initial data. (a bit overkill, consider hosting them on CDN)

- **SignalRServer** – hosts SignalR hubs used by the UI for real-time events (e.g., refreshing history views).

- **EmailService** – sends emails/notifications (internal service).

- **HangfireService** – background jobs/scheduling; uses RabbitMQ (via MassTransit) for messaging/event handling.

- **BackupService** – handles backup-related tasks (e.g., database backups).

- **Commons** (shared libraries) - custom implementation of Mediator pattern, ExceptionHandlerMiddleware, BearerTokenHandler etc.

### Authorization
(**NOTE:** it's a work in progress)

- **OAuthServer** – identity/token authority used by internal clients/services. (for now can work internally in docker)

- **JwtAuth** – shared auth-related components/utilities. (possible removal)

### Infrastructure (containers)

- SQL Server – used by CitiesService, BackupService.

- PostgreSQL – available for services that use it (optional depending on your config). (Future replacement of MS SQL)

- MongoDB – used by WeatherHistoryService, IconService,Hangfire and SignalRServer.

- Redis – caching.

- RabbitMQ – messaging transport (used with MassTransit).

- Seq – centralized logging (Serilog sink).

- HealthChecks UI – dashboards health endpoints across services.

- (Optional dev tooling: Portainer, Jenkins)

---

### Communication patterns

- HTTP REST between WeatherSite and most internal services.

- gRPC between WeatherSite and CitiesGrpcService.

- SignalR for real-time updates from SignalRServer to the browser.

- Message bus (MassTransit over RabbitMQ) for async/event-driven flows (e.g., background processing).

---

### Deployment modes
- **Local development:** build and run everything with Docker Compose (some services may use HTTPS locally).
- **AWS dev deployment (single VM):**
  - images built locally and pushed to AWS ECR
  - EC2 pulls and runs containers using Docker Compose
  - nginx provides a single HTTP entrypoint and routes `/` to WeatherSite and the SignalR hub route

---

### Health & observability
- Each service expose a `/health` endpoint.
- HealthChecks UI aggregates health endpoints for a quick status overview.
- Serilog outputs to console and can ship logs to Seq.

---

## Simplified backlog

- migrated from .NET 5 to .NET 10
- solution structure changed to 'mimic microservices' approach
- fixed general errors that prevented projects from being operational when running on Docker and EC2
- general overall refactor and cleanup
- added health checks for each app and configured Watchdog
- added OAuth identity server (using `duende` package for test purposes) // still needs work
- multi-stage Dockerfiles for all services
- backup service added (with Hangfire integration)
- dev deploy to AWS
---

## Setup Guide (Local + AWS EC2 Dev Deploy)

This guide describes:
- how to run the system locally (Docker Compose on Debian based Linux)
- how to publish images to AWS ECR (using AWS CLI)
- how to deploy and run on a single EC2 instance (Docker Compose on the VM)

> **Repo layout note**
> Deployment artifacts (scripts, compose files, nginx config) live in: `src/deploy/`
> 
> Compose is executed with `--project-directory src` so that relative mounts like `./Authorization/...` still work.

---

### Prerequisites

#### Local machine (Debian-based Linux)
- `git`
- `.NET SDK` (`dotnet`)
- Docker Engine + Docker Compose plugin (`docker compose`)
- AWS CLI v2 (for pushing to ECR)
- `libman` (WeatherSite static assets)
- build/deploy scripts will guide you if some required packages are missing
- ensure each service has config for its runtime environment (appsettings.json + appsettings.Development.json / appsettings.Production.json as needed)

#### AWS account
- IAM user or SSO credentials for AWS CLI usage
- ECR access (push from local machine)
- EC2 instance role to pull images from ECR

#### External Api key
-  Api key from: [open weather map](https://home.openweathermap.org/api_keys)
   set OpenWeatherMap:ApiKey via environment variable (recommended) or in WeatherService/appsettings.json (dev only).

---

### Local Setup

#### Clone the repository
```bash
git clone git@github.com:mirusser/Simple-Weather-Site.git
cd Simple-Weather-Site/src
```
#### WeatherSite static files (LibMan)
WeatherSite relies on LibMan-managed assets, restore them:

Install `libman` tool (if not installed):
```bash
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
```

Then restore in the WeatherSite project directory:
```bash
cd WeatherSite/Site
libman restore
cd ../../
```

#### Service startup notes
- CitiesService: on startup it can create DB, run migrations and seed data.
Ensure the configured SQL Server user has permissions to create DB + apply migrations.
- IconService: on startup it can create Mongo DB/collections and seed them.

---

### Local Run (Docker)
Make scripts executable:
```bash
cd <YOUR_REPO>/src/deploy
chmod +x build-run-locally.sh
chmod +x push-ecr.sh
chmod +x upload-to-ec2.sh
```

Build images + run local compose

From repo root or anywhere:
```bash
cd <YOUR_REPO>/src/deploy
./build-run-locally.sh
```

Check:
```bash
docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'
```

Verify the site is running on your browser, check health checks

---

### AWS Dev Deployment (EC2 + ECR + Docker Compose)

This is a “single VM” deployment:
You build locally → push images to ECR.

EC2 pulls images from ECR and runs everything with Docker Compose.

Reverse proxy (nginx) exposes port 80 on the instance

> This is meant for dev/testing. Not HA, not autoscaled, not hardened.

---

#### Setup AWS CLI on your local machine (debian-based Linux) 

##### Installation

```bash
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install
```

To check version:
```bash
aws --version
```

To update:
```bash
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install --bin-dir /usr/local/bin --install-dir /usr/local/aws-cli --update
```

##### Local configuration profile
Set up:
```bash
aws configure --profile <profile_name>
```

Verify:
```bash
aws sts get-caller-identity --profile <profile_name>
```

Get account ID:
```bash
aws sts get-caller-identity --query Account --output text --profile <profile_name>
```

You can manually create ECR (Elastic Container Registry) repos,
(but is not necessary as scripts create those later:
```bash
aws ecr create-repository \
  --repository-name <repository_name> \
  --image-scanning-configuration scanOnPush=true \
  --profile <profile_name>
```
---

### AWS Network basics
- Use the default VPC (fine for dev), and a public subnet. 
- Create a **Security Group for EC2**:
    - inbound: `80` from your IP (public test: 0.0.0.0/0 (and optionally ::/0 if IPv6))
    - inbound: `22` from _your IP only_ (SSH)
    - No other inbound ports.
    - Outbound: allow all (default is fine). 

> Keep infra ports (DB, Redis, RabbitMQ, etc.) internal-only (do not open them to the internet).

---

### EC2 instance launch
Recommended for your stack (lots of infra containers):

- Instance type: **t3.xlarge** (4 vCPU, 16GB RAM)
(SQL Server, Mongo, Redis etc. need RAM)
- AMI: Debian 13
- Key pair type: ED25519 (good default)
- IAM role for the instance, attach an IAM role that can pull from ECR, e.g.:
`AmazonEC2ContainerRegistryReadOnly`

#### EC2 SSH connection
On AWS create and then download key pair (`*.pem` file)

(Debian default user is typically: `admin`)

Connect using the key:
```bash
ssh -i sws-ec2-key-pair.pem admin@<EC2_PUBLIC_IPv4>
```

If SSH complain:
- `UNPROTECTED PRIVATE KEY FILE` (permissions too open)

Fix it with:
```bash
chmod 400 sws-ec2-key-pair.pem
# (chmod 600 would also work)
```

#### EC2 instance configuration
Install: Docker + docker compose plugin.
On EC2 instance:
```bash
sudo apt-get update
sudo apt-get install -y ca-certificates curl gnupg

sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/debian/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

CODENAME=$(. /etc/os-release && echo "$VERSION_CODENAME")
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian ${CODENAME} stable" \
  | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
```

Enable/start docker:
```bash
sudo systemctl enable --now docker
```

Allow docker without sudo (then reconnect):
```bash
sudo usermod -aG docker $USER
exit
```

After reconnect verify:
```bash
docker --version
docker compose version
docker ps
```

#### Prepare EC2 Deploy Directory
On EC2 instance create a directory for solution/deploy:
```bash
sudo mkdir -p /opt/sws
sudo chown -R $USER:$USER /opt/sws
cd /opt/sws
```

--- 

### Build + Push Images to ECR (Local Machine)

From repo:
```bash
cd <YOUR_REPO>/src/deploy
./build-run-locally.sh # if not build yet do it now
./push-ecr.sh
```
> `push-ecr.sh` script pushes images and writes .env.prod 
>
> (location depends on your script; typically in src/deploy/.env.prod).

#### Upload Deploy Artifacts to EC2 (Local Machine)

From repo:
```bash
cd <YOUR_REPO>/src/deploy
./upload-to-ec2.sh <EC2_PUBLIC_IP>
```

Options:
>--dry-run : prints what would run
>
>--skip-env: does not overwrite .env.* on EC2

---

### Run Deployment on EC2

SSH into EC2:
```bash
ssh -i sws-ec2-key-pair.pem admin@<EC2_PUBLIC_IP>
cd /opt/sws
./deploy-ec2.sh
```

Check containers:
```bash
docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'
```

Check health locally on EC2:
```bash
curl -i http://localhost/health
curl -i http://localhost/
```

Check from your local machine:
```bash
curl -i http://<EC2_PUBLIC_IP>/health
```

---

### Troubleshooting
Containers stuck restarting, use logs:
```bash
docker logs --tail 200 <container_name>
```

Stop and remove all docker containers: 
(doesn't remove volumes)
```bash
docker ps -aq | xargs -r docker rm -f
```

Manually run docker compose:
```bash
docker compose \
  --project-directory ./src \
  --env-file ./src/deploy/.env.prod \
  -f ./src/deploy/docker-compose.prod.yml \
  up -d
```

---

## Roadmap ideas:

- CI/CD pipeline (Jenkins/Aspire (?)) or alternatives)
- there is still some hardcoded stuff and some TODOs to be handled
- Add tests for each project (xUnit)
- MCP server for projects (demo: [My repo for MCP](https://github.com/mirusser/MCP))
- continue with 'refactor', 'cleanup' and modernization
- deeper AWS-native migration (Use AWS services e.g. SQS, DynamoDB, automatic backup of databases etc.)
- It would be nice to be able to download historic data in various file types (e.g. PDF, excel, csv, Markdown, HTML etc.)
- Configurable via page (something like mini admin panel, at least for URLs to be configurable during runtime)
- in weather prediction will be good if there would be an option to get user location (by IP I guess, microservice for that?) - so there is no need for a user to manually search for city
- feature: unod (dunno what I can undo yet, TODO: think about it)
- handle more requests/endpoints from openweather api (more features yay!)
- switch from SQL server to postgres
- switch from MassTransit to OpenTransit (when the package will be ready)
- consider ditching `duende` (for alternative, what? do research)
- update (!) UI/frontend (Blazor maybe (?))
- elastic search for cities service searches (and weather site (?)) (and graphql endpoint (?))
- update HealthChecks UI
- overview and general upgrade of 'security'
- GraphQL endpoint (for cities service (?))

### Things to check out:

- **Prometheus and Grafana:** For more complex scenarios, especially when you need more than just health status (e.g., metrics and detailed monitoring), using Prometheus for collecting metrics and Grafana for visualization can be a powerful combination. You would use Prometheus exporters to expose metrics from your services, including health check statuses, and then aggregate and visualize them in Grafana.

- **Consul:** Offers service discovery and health checking capabilities. You can use Consul to keep track of the health of various services in your infrastructure. It requires more setup and infrastructure changes but is powerful for microservices architectures.

---

































