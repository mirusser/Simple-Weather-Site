#!/usr/bin/env bash
set -Eeuo pipefail
trap 'echo "Error on line $LINENO: $BASH_COMMAND"' ERR

# Where compose files live on EC2
APP_DIR="${APP_DIR:-/opt/sws}"
cd "$APP_DIR"

AWS_REGION="${AWS_REGION:-us-east-1}"
ECR_REGISTRY="${ECR_REGISTRY:?Set ECR_REGISTRY in .env.prod}"
COMPOSE_PROJECT_NAME="${COMPOSE_PROJECT_NAME:-sws}"

NETWORK_NAME="${NETWORK_NAME:-sws-containers-bridge-network}"

echo "==> Ensuring docker network exists: $NETWORK_NAME"
if ! docker network ls --format '{{.Name}}' | grep -Fxq "$NETWORK_NAME"; then
    docker network create -d bridge "$NETWORK_NAME"
fi

echo "==> Logging into ECR: $ECR_REGISTRY"
aws ecr get-login-password --region "$AWS_REGION" \
        | docker login --username AWS --password-stdin "$ECR_REGISTRY" >/dev/null

echo "==> Starting infra stack"
docker compose \
    --project-name "$COMPOSE_PROJECT_NAME-infra" \
    --env-file .env.infra \
    -f docker-compose.infra.prod.yml \
    up -d

echo "==> Pulling app images"
docker compose \
    --project-name "$COMPOSE_PROJECT_NAME-app" \
    --env-file .env.prod \
    -f docker-compose.prod.yml \
    pull

echo "==> Starting app stack"
docker compose \
    --project-name "$COMPOSE_PROJECT_NAME-app" \
    --env-file .env.prod \
    -f docker-compose.prod.yml \
    up -d

echo "==> Done."
echo "ALB target should be the EC2 instance on port 80 (weathersite is published as 80:80)."
docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'
