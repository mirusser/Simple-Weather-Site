# Setup Guide (Local + AWS EC2 Dev Deploy)

This guide describes:

- how to run the system locally with Docker Compose on Debian-based Linux
- how to publish images to AWS ECR with AWS CLI
- how to deploy and run on a single EC2 instance with Docker Compose

Deployment artifacts live in this directory: scripts, compose files, env examples, and nginx config.

Compose is executed with `--project-directory src` so relative mounts like `./Authorization/...` still resolve from the solution `src` directory.

---

## Prerequisites

### Local Machine

- `git`
- `.NET SDK` (`dotnet`)
- Docker Engine + Docker Compose plugin (`docker compose`)
- AWS CLI v2 for ECR pushes
- `libman` for WeatherSite static assets
- service config for the target runtime environment

The local build/deploy scripts also check for required tools and print guidance when something is missing.

### AWS Account

- IAM user or SSO credentials for AWS CLI usage
- ECR access for pushing images from the local machine
- EC2 instance role with ECR pull permissions, for example `AmazonEC2ContainerRegistryReadOnly`

### External API Key

Create an API key at [OpenWeatherMap](https://home.openweathermap.org/api_keys) and provide it through configuration. Prefer environment variables for non-local use.

---

## Local Setup

Clone the repository:

```bash
git clone git@github.com:mirusser/Simple-Weather-Site.git
cd Simple-Weather-Site/src
```

Restore WeatherSite static files:

```bash
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
cd WeatherSite/Site
libman restore
cd ../../
```

Service startup notes:

- CitiesService can create the database, apply provider-specific PostgreSQL or SQL Server EF Core migrations, and seed data at startup. The configured DB user needs database create/migration permissions.
- IconService can create MongoDB collections and seed initial data at startup.

---

## Local Run (Docker)

Make scripts executable:

```bash
cd <YOUR_REPO>/src/deploy
chmod +x build-run-locally.sh
chmod +x push-ecr.sh
chmod +x upload-to-ec2.sh
```

Build images and start the local compose stack:

```bash
cd <YOUR_REPO>/src/deploy
./build-run-locally.sh
```

Check running containers:

```bash
docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'
```

Then open the site in a browser and check service health endpoints.

---

## AWS Dev Deployment (EC2 + ECR + Docker Compose)

This is a single-VM dev/test deployment. You build locally, push images to ECR, upload deployment artifacts to EC2, and run Docker Compose on the VM. Nginx exposes port `80` on the instance.

This setup is not high availability, autoscaled, or hardened.

### Setup AWS CLI Locally

Install AWS CLI v2 on Debian-based Linux:

```bash
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install
```

Check version:

```bash
aws --version
```

Update AWS CLI:

```bash
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install --bin-dir /usr/local/bin --install-dir /usr/local/aws-cli --update
```

Configure a local profile:

```bash
aws configure --profile <profile_name>
```

Verify credentials:

```bash
aws sts get-caller-identity --profile <profile_name>
aws sts get-caller-identity --query Account --output text --profile <profile_name>
```

You can create ECR repositories manually, but `push-ecr.sh` creates missing repositories:

```bash
aws ecr create-repository \
  --repository-name <repository_name> \
  --image-scanning-configuration scanOnPush=true \
  --profile <profile_name>
```

### AWS Network Basics

- Use the default VPC and a public subnet for this dev setup.
- EC2 security group:
  - inbound `80` from your IP, or `0.0.0.0/0` for public testing
  - inbound `22` from your IP only
  - no other inbound ports
  - outbound allow all

Keep database, Redis, RabbitMQ, Seq, and other infra ports internal-only.

### EC2 Instance Launch

Recommended instance shape:

- Instance type: `t3.xlarge` (4 vCPU, 16 GB RAM)
- AMI: Debian 13
- Key pair type: ED25519
- IAM role: attach ECR read permissions, for example `AmazonEC2ContainerRegistryReadOnly`

Connect with SSH:

```bash
ssh -i sws-ec2-key-pair.pem admin@<EC2_PUBLIC_IPv4>
```

If SSH reports `UNPROTECTED PRIVATE KEY FILE`, fix the key permissions:

```bash
chmod 400 sws-ec2-key-pair.pem
```

### EC2 Instance Configuration

Install Docker and the Docker Compose plugin on the EC2 instance:

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

Enable and start Docker:

```bash
sudo systemctl enable --now docker
```

Allow Docker without sudo, then reconnect:

```bash
sudo usermod -aG docker $USER
exit
```

After reconnecting, verify Docker:

```bash
docker --version
docker compose version
docker ps
```

Create the EC2 deploy directory:

```bash
sudo mkdir -p /opt/sws
sudo chown -R $USER:$USER /opt/sws
cd /opt/sws
```

### Build and Push Images to ECR

From the local repo:

```bash
cd <YOUR_REPO>/src/deploy
./build-run-locally.sh
./push-ecr.sh
```

`push-ecr.sh` pushes images and writes `.env.prod`. Infra secrets/config are supplied separately through `.env.infra`.

### Upload Deploy Artifacts to EC2

From the local repo:

```bash
cd <YOUR_REPO>/src/deploy
./upload-to-ec2.sh <EC2_PUBLIC_IP>
```

Options:

- `--dry-run` prints what would run
- `--skip-env` does not overwrite `.env.*` files on EC2

Make sure both `.env.prod` and `.env.infra` exist on EC2 before running the deploy script. Example templates are available as `.env.example.prod` and `.env.example.infra`.

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

## Troubleshooting

Containers stuck restarting:

```bash
docker logs --tail 200 <container_name>
```

Stop and remove all containers without removing volumes:

```bash
docker ps -aq | xargs -r docker rm -f
```

Manually start the infra stack first:

```bash
docker compose \
  --project-name sws-infra \
  --env-file .env.infra \
  -f docker-compose.infra.prod.yml \
  up -d
```

Then start the app stack:

```bash
docker compose \
  --project-name sws-app \
  --env-file .env.prod \
  -f docker-compose.prod.yml \
  up -d
```
