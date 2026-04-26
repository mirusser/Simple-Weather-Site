#!/usr/bin/env bash
set -Eeuo pipefail
trap 'echo "Error on line $LINENO: $BASH_COMMAND"' ERR

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

PROJECT_NAME="sws-infra"
NETWORK_NAME="sws-containers-bridge-network"
ENV_FILE=".env.infra"
ENV_EXAMPLE=".env.example.infra"
COMPOSE_FILE="docker-compose.infra.prod.yml"

INFRA_VOLUME_DIRS=(
    /opt/sws/volumes/postgresql
    /opt/sws/volumes/mongo/data/db
    /opt/sws/volumes/mssql/data
    /opt/sws/volumes/redis/data
    /opt/sws/volumes/seq/data
    /opt/sws/volumes/prometheus/data
    /opt/sws/volumes/grafana/data
    /opt/sws/volumes/loki/data
    /opt/sws/volumes/rabbitmq/data
)

require_docker() {
    if ! command -v docker >/dev/null 2>&1; then
        echo "docker could not be found. Please install Docker."
        exit 1
    fi

    if ! docker info >/dev/null 2>&1; then
        echo "Docker daemon is not reachable. Is Docker running, and do you have permission to access it?"
        exit 1
    fi

    if ! docker compose version >/dev/null 2>&1; then
        echo "docker compose could not be found. Please install the Docker Compose plugin."
        exit 1
    fi
}

ensure_env_file() {
    if [[ -f "$ENV_FILE" ]]; then
        return
    fi

    cp "$ENV_EXAMPLE" "$ENV_FILE"
    echo "Created $SCRIPT_DIR/$ENV_FILE from $ENV_EXAMPLE."
    echo "Fill POSTGRES_PASSWORD, MSSQL_SA_PASSWORD, and GRAFANA_ADMIN_PASSWORD in $ENV_FILE, then rerun this script."
    exit 1
}

ensure_network() {
    echo "==> Ensuring docker network exists: $NETWORK_NAME"
    if ! docker network ls --format '{{.Name}}' | grep -Fxq "$NETWORK_NAME"; then
        docker network create -d bridge "$NETWORK_NAME"
    fi
}

ensure_volume_dirs() {
    echo "==> Ensuring infra volume directories exist"
    if mkdir -p "${INFRA_VOLUME_DIRS[@]}" 2>/dev/null; then
        return
    fi

    if ! command -v sudo >/dev/null 2>&1; then
        echo "Could not create infra volume directories under /opt/sws/volumes, and sudo is not available."
        exit 1
    fi

    sudo mkdir -p "${INFRA_VOLUME_DIRS[@]}"
}

validate_compose() {
    echo "==> Validating infra compose environment"
    docker compose \
        --project-name "$PROJECT_NAME" \
        --env-file "$ENV_FILE" \
        -f "$COMPOSE_FILE" \
        config --environment >/dev/null

    echo "==> Validating rendered infra compose config"
    docker compose \
        --project-name "$PROJECT_NAME" \
        --env-file "$ENV_FILE" \
        -f "$COMPOSE_FILE" \
        config >/dev/null
}

start_infra() {
    echo "==> Starting infra stack"
    docker compose \
        --project-name "$PROJECT_NAME" \
        --env-file "$ENV_FILE" \
        -f "$COMPOSE_FILE" \
        up -d
}

print_status() {
    echo "==> Infra container status"
    docker compose \
        --project-name "$PROJECT_NAME" \
        --env-file "$ENV_FILE" \
        -f "$COMPOSE_FILE" \
        ps
}

ensure_env_file
require_docker
ensure_network
ensure_volume_dirs
validate_compose
start_infra
"$SCRIPT_DIR/ensure-mongo-replica-set.sh"
print_status
