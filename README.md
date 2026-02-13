# Simple Weather Site

## Overview (TODO)

...

---

## Simplified backlog

- Migrated from .NET 5 to .NET 10
- Changed folder structure to be more usable and 'friendly'
- Fixed general errors that prevented project from being fully operational when running on Docker
- Some minor refactor
- Added health checks for each app and configured Watchdog
- Added (a little janky but still) OAuth identity server (using `duende` package for test purposes)
- dev deploy to AWS
---

## Setup Guide (Local + AWS EC2 Dev Deploy)

This guide describes:
- how to run the system locally (Docker Compose)
- how to publish images to AWS ECR
- how to deploy and run on a single EC2 instance (Docker Compose on the VM)

> **Repo layout note**
> Deployment artifacts (scripts, compose files, nginx config) live in: `src/deploy/`
> 
> Compose is executed with `--project-directory src` so that relative mounts like `./Authorization/...` still work.

---

### Prerequisites

#### Local machine (Debian-based Linux recommended)
- `git`
- `.NET SDK` (`dotnet`)
- Docker Engine + Docker Compose plugin (`docker compose`)
- AWS CLI v2 (for pushing to ECR)
- `libman` (WeatherSite static assets)

#### AWS account
- IAM user or SSO credentials for AWS CLI usage
- ECR access (push from local machine)
- EC2 instance role to pull images from ECR

#### External Api key
-  Api key from: [open weather map](https://home.openweathermap.org/api_keys)
  and put it in `appsettings.Production.json` of `WeatherService`

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

Restore in the WeatherSite project directory:
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

#### Setup up AWS CLI on your local machine (debian-based linux) 

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

Get account Id:
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
./build-run-locally.sh
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
Containers stuck restarting

Use logs:
```bash
docker logs --tail 200 <container_name>
```

---

## Features that I may add:

- Setup build with jenkins (CI/CD pipeline)
- Add tests
- Migrate fully to AWS (Use AWS services e.g. SQS, DynamoDB, automatic backup of databases etc.)
- It would be nice to be able to download historic data in various file types (e.g. pdg, excel, csv, html, etc)
- Configurable via page (something like mini admin panel, at least for urls to be configurable during runtime)
- in weather prediction will be good if there would be an option to get user location (by ip I guess, microservice for that?)
- feature: unod (dunno what I can undo yet, TODO: think about it)
- handle more requests/endpoints from openweather api
- least important but would be nice to redo frontend (tho I lack in that area, maybe use blazor)
- switch from sql server to postgres
- switch from MassTransit to OpenTransit (when the package will be ready)
- consider ditching `duende`

### Things to check out:

- **Prometheus and Grafana:** For more complex scenarios, especially when you need more than just health status (e.g., metrics and detailed monitoring), using Prometheus for collecting metrics and Grafana for visualization can be a powerful combination. You would use Prometheus exporters to expose metrics from your services, including health check statuses, and then aggregate and visualize them in Grafana.

- **Consul:** Offers service discovery and health checking capabilities. You can use Consul to keep track of the health of various services in your infrastructure. It requires more setup and infrastructure changes but is powerful for microservices architectures.

---

































