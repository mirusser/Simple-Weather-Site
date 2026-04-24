#!/usr/bin/env bash
set -Eeuo pipefail
trap 'echo "Error on line $LINENO: $BASH_COMMAND"' ERR

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
K8S_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
OVERLAY_DIR="$K8S_DIR/overlays/minikube"

MINIKUBE_PROFILE="${MINIKUBE_PROFILE:-minikube}"
MINIKUBE_CPUS="${MINIKUBE_CPUS:-6}"
MINIKUBE_MEMORY="${MINIKUBE_MEMORY:-8192}"
IMAGE_TAG="minikube"
K8S_NAMESPACE="${K8S_NAMESPACE:-sws}"
SKIP_BUILD="${SKIP_BUILD:-0}"

check_dependencies() {
    local dependencies=("docker" "minikube" "cp")
    for dep in "${dependencies[@]}"; do
        command -v "$dep" >/dev/null 2>&1 || {
            echo "Missing dependency: $dep"
            exit 1
        }
    done
}

kube() {
    minikube -p "$MINIKUBE_PROFILE" kubectl -- "$@"
}

ensure_env_file() {
    local example_file="$1"
    local target_file="$2"

    if [[ -f "$target_file" ]]; then
        return
    fi

    cp "$example_file" "$target_file"
    echo "Created $target_file from $(basename "$example_file")."
}

ensure_overlay_env_files() {
    ensure_env_file \
        "$OVERLAY_DIR/app-config.example.env" \
        "$OVERLAY_DIR/app-config.env"
    ensure_env_file \
        "$OVERLAY_DIR/app-secrets.example.env" \
        "$OVERLAY_DIR/app-secrets.env"
}

ensure_minikube_running() {
    local status
    status="$(minikube status -p "$MINIKUBE_PROFILE" --format='{{.Host}} {{.Kubelet}} {{.APIServer}}' 2>/dev/null || true)"

    if [[ "$status" == "Running Running Running" ]]; then
        return
    fi

    echo "==> Starting Minikube profile '$MINIKUBE_PROFILE'"
    minikube start \
        -p "$MINIKUBE_PROFILE" \
        --cpus="$MINIKUBE_CPUS" \
        --memory="$MINIKUBE_MEMORY"
}

enable_ingress() {
    echo "==> Enabling nginx ingress addon"
    minikube -p "$MINIKUBE_PROFILE" addons enable ingress >/dev/null

    echo "==> Waiting for ingress controller"
    kube wait \
        --namespace ingress-nginx \
        --for=condition=Ready pod \
        --selector=app.kubernetes.io/component=controller \
        --timeout=300s
}

build_images() {
    if [[ "$SKIP_BUILD" == "1" ]]; then
        echo "==> Skipping image build"
        return
    fi

    echo "==> Building application images with tag '$IMAGE_TAG'"
    MINIKUBE_PROFILE="$MINIKUBE_PROFILE" IMAGE_TAG="$IMAGE_TAG" \
        "$SCRIPT_DIR/build-images-minikube.sh"
}

wait_for_rollout() {
    local kind="$1"
    local name="$2"

    echo "==> Waiting for $kind/$name"
    kube rollout status "$kind/$name" --namespace "$K8S_NAMESPACE" --timeout=300s
}

check_dependencies
ensure_overlay_env_files
ensure_minikube_running
enable_ingress
build_images

echo "==> Resetting Mongo replica-set init job"
kube delete job mongo-rs-init --namespace "$K8S_NAMESPACE" --ignore-not-found >/dev/null 2>&1 || true

echo "==> Ensuring namespace '$K8S_NAMESPACE' exists"
kube apply -f "$K8S_DIR/base/namespace.yaml"

echo "==> Applying Kubernetes manifests"
kube apply -k "$OVERLAY_DIR"

wait_for_rollout statefulset mongo
wait_for_rollout deployment sqlserver
wait_for_rollout deployment redis
wait_for_rollout deployment rabbitmq
wait_for_rollout deployment seq

echo "==> Waiting for mongo-rs-init job"
kube wait \
    --for=condition=complete \
    job/mongo-rs-init \
    --namespace "$K8S_NAMESPACE" \
    --timeout=300s

wait_for_rollout deployment oauthserver
wait_for_rollout deployment citiesservice
wait_for_rollout deployment citiesgrpcservice
wait_for_rollout deployment weatherservice
wait_for_rollout deployment weatherhistoryservice
wait_for_rollout deployment iconservice
wait_for_rollout deployment signalrserver
wait_for_rollout deployment emailservice
wait_for_rollout deployment hangfireservice
wait_for_rollout deployment weathersite
wait_for_rollout deployment gateway

MINIKUBE_IP="$(minikube -p "$MINIKUBE_PROFILE" ip)"

echo
echo "Deployment completed."
echo "Application URL: http://$MINIKUBE_IP/"
echo
echo "Pods:"
kube get pods --namespace "$K8S_NAMESPACE"
echo
echo "Ingress:"
kube get ingress --namespace "$K8S_NAMESPACE"
