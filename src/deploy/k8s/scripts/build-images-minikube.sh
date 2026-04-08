#!/usr/bin/env bash
set -Eeuo pipefail
trap 'echo "Error on line $LINENO: $BASH_COMMAND"' ERR

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_DIR="$(cd "$SCRIPT_DIR/../../../.." && pwd)"
SRC_DIR="$REPO_DIR/src"

MINIKUBE_PROFILE="${MINIKUBE_PROFILE:-minikube}"
IMAGE_TAG="minikube"

check_dependencies() {
    local dependencies=("docker" "minikube")
    for dep in "${dependencies[@]}"; do
        command -v "$dep" >/dev/null 2>&1 || {
            echo "Missing dependency: $dep"
            exit 1
        }
    done
}

ensure_cluster_running() {
    local status
    status="$(minikube status -p "$MINIKUBE_PROFILE" --format='{{.Host}} {{.Kubelet}} {{.APIServer}}' 2>/dev/null || true)"

    if [[ "$status" != "Running Running Running" ]]; then
        echo "Minikube profile '$MINIKUBE_PROFILE' is not running."
        echo "Start it first or run deploy-minikube.sh."
        exit 1
    fi
}

build_image() {
    local image_name="$1"
    local dockerfile="$2"

    echo "==> Building $image_name:$IMAGE_TAG"
    docker build \
        -t "$image_name:$IMAGE_TAG" \
        -f "$dockerfile" \
        "$SRC_DIR"
}

check_dependencies
ensure_cluster_running

eval "$(minikube -p "$MINIKUBE_PROFILE" docker-env)"

build_image "oauthserver" "$SRC_DIR/Authorization/OAuthServer/Kubernetes.dockerfile"
build_image "citiesservice" "$SRC_DIR/CitiesService/CitiesService.Api/Kubernetes.dockerfile"
build_image "citiesgrpcservice" "$SRC_DIR/CitiesService/CitiesGrpcService/Dockerfile"
build_image "weatherservice" "$SRC_DIR/WeatherService/Dockerfile"
build_image "weatherhistoryservice" "$SRC_DIR/WeatherHistoryService/Dockerfile"
build_image "iconservice" "$SRC_DIR/IconService/IconService.Api/Dockerfile"
build_image "signalrserver" "$SRC_DIR/SignalRServer/Dockerfile"
build_image "emailservice" "$SRC_DIR/EmailService/EmailService.Api/Dockerfile"
build_image "hangfireservice" "$SRC_DIR/HangfireService/Dockerfile"
build_image "weathersite" "$SRC_DIR/WeatherSite/Site/Dockerfile"

echo
echo "Built local application images inside Minikube profile '$MINIKUBE_PROFILE'."
