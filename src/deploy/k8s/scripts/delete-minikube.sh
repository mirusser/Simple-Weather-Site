#!/usr/bin/env bash
set -Eeuo pipefail
trap 'echo "Error on line $LINENO: $BASH_COMMAND"' ERR

MINIKUBE_PROFILE="${MINIKUBE_PROFILE:-minikube}"
K8S_NAMESPACE="${K8S_NAMESPACE:-sws}"

check_dependencies() {
    command -v minikube >/dev/null 2>&1 || {
        echo "Missing dependency: minikube"
        exit 1
    }
}

kube() {
    minikube -p "$MINIKUBE_PROFILE" kubectl -- "$@"
}

ensure_cluster_running() {
    local status
    status="$(minikube status -p "$MINIKUBE_PROFILE" --format='{{.Host}} {{.Kubelet}} {{.APIServer}}' 2>/dev/null || true)"

    if [[ "$status" != "Running Running Running" ]]; then
        echo "Minikube profile '$MINIKUBE_PROFILE' is not running. Nothing to delete."
        exit 0
    fi
}

check_dependencies
ensure_cluster_running

echo "==> Deleting namespace '$K8S_NAMESPACE'"
kube delete namespace "$K8S_NAMESPACE" --ignore-not-found --wait=true

echo "Kubernetes resources removed from Minikube profile '$MINIKUBE_PROFILE'."
