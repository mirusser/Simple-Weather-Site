#!/usr/bin/env bash

# This script is meant to run locally to push already build docker image to AWS ECR
# Should be executed after `build-run-locally.sh` script\

# How to run this script:
# ./push-ecr.sh

# Colors for better readability
GREEN='\033[0;32m' # for success messages
YELLOW='\033[0;33m' # for warning messages
RED='\033[0;31m' # for error messages
BLUE='\033[0;34m' # for custom informative messages
NC='\033[0m' # No Color - for default/informative messages

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SRC_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"     # <repo>/src
BASE_DIR="${1:-$SRC_DIR}"

# Where to write .env.prod (override if you want)
ENV_OUT="${ENV_OUT:-$SCRIPT_DIR/.env.prod}"

# --- AWS/ECR settings ---
PUSH_TO_ECR="${PUSH_TO_ECR:-1}"
AWS_PROFILE="${AWS_PROFILE:-sws}" # optional; set to empty if you don't use profiles
AWS_REGION="${AWS_REGION:-us-east-1}" # pick your region
ECR_PREFIX="${ECR_PREFIX:-sws}" # namespace/prefix in ECR, e.g. sws/citiesservice

# Image tag (git sha preferred)
IMAGE_TAG="${IMAGE_TAG:-$(
    git -C "$BASE_DIR" rev-parse --short HEAD 2>/dev/null || date +%Y%m%d%H%M
)}"

SERVICES=(
    "citiesservice"
    "citiesgrpcservice"
    "weatherservice"
    "weatherhistoryservice"
    "emailservice"
    "iconservice"
    "signalrserver"
    "hangfireservice"
    "backupservice"
    "weathersite"
)

# strict mode + better error handling
# -E  : functions inherit ERR trap
# -e  : exit immediately on error (non-zero exit code)
# -u  : treat unset variables as an error
# -o pipefail : fail a pipeline if any command in it fails
set -Eeuo pipefail
# Print a helpful message when something fails (line number + failing command)
trap 'echo -e "${RED}Error on line $LINENO: $BASH_COMMAND${NC}"' ERR

check_dependencies() {

    # NOTE:
    # We check only "user-level" commands here (no sudo required):
    local dependencies=("aws" "docker")
    for dep in "${dependencies[@]}"; do
        command -v "$dep" &>/dev/null || { echo "$dep could not be found."; exit 1; }
    done
    docker info &>/dev/null || { echo -e "${RED}Docker daemon not reachable.${NC}"; exit 1; }
}

aws_cli() {
    # Helper so we can support profile/no-profile cleanly
    if [ -n "${AWS_PROFILE:-}" ]; then
        aws --profile "$AWS_PROFILE" "$@"
    else
        aws "$@"
    fi
}

ecr_login() {
    [ "$PUSH_TO_ECR" = "1" ] || return 0

    AWS_ACCOUNT_ID="$(aws_cli sts get-caller-identity --query Account --output text)"
    : "${AWS_ACCOUNT_ID:?Failed to resolve AWS_ACCOUNT_ID}"

    ECR_REGISTRY="$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com"

    echo -e "${BLUE}Logging into ECR: $ECR_REGISTRY${NC}"
    aws_cli ecr get-login-password --region "$AWS_REGION" \
        | docker login --username AWS --password-stdin "$ECR_REGISTRY" >/dev/null
}

ensure_ecr_repo() {
    local repo="$1" # e.g. sws/citiesservice
    if ! aws_cli ecr describe-repositories --repository-names "$repo" --region "$AWS_REGION" >/dev/null 2>&1; then
        echo -e "${YELLOW}ECR repo $repo not found, creating...${NC}"
        aws_cli ecr create-repository \
            --repository-name "$repo" \
            --image-scanning-configuration scanOnPush=true \
            --region "$AWS_REGION" >/dev/null
    fi
}

tag_and_push_image() {
    [ "$PUSH_TO_ECR" = "1" ] || return 0

    # Args:
        #  $1 local image name (e.g. citiesservice)
        #  $2 repo name (e.g. citiesservice) -> will become $ECR_PREFIX/citiesservice
    local local_image="$1"
    local repo_name="$2"

    if ! docker image inspect "$local_image:latest" >/dev/null 2>&1; then
        echo -e "${RED}Local image not found: $local_image:latest (run build first)${NC}"
        exit 1
    fi

    ensure_ecr_repo "$ECR_PREFIX/$repo_name"

    local remote_image="$ECR_REGISTRY/$ECR_PREFIX/$repo_name:$IMAGE_TAG"
    echo -e "${BLUE}Pushing $local_image -> $remote_image${NC}"
    docker tag "$local_image:latest" "$remote_image"
    docker push "$remote_image"

    docker tag "$local_image:latest" "$ECR_REGISTRY/$ECR_PREFIX/$repo_name:latest"
    docker push "$ECR_REGISTRY/$ECR_PREFIX/$repo_name:latest"
}

write_env_prod_file() {
    [ "$PUSH_TO_ECR" = "1" ] || return 0

    cat > "$ENV_OUT" <<EOF
AWS_REGION=$AWS_REGION
ECR_REGISTRY=$ECR_REGISTRY
ECR_PREFIX=$ECR_PREFIX
IMAGE_TAG=$IMAGE_TAG
EOF

    echo -e "${GREEN}Wrote $ENV_OUT${NC}"
}

# ---- Script entrypoint (runs in order) ----

# Validate tools and environment
check_dependencies
# Login to ECR once (only if pushing is enabled)
ecr_login

: "${ECR_REGISTRY:?ECR_REGISTRY not set}"

for svc in "${SERVICES[@]}"; do
    tag_and_push_image "$svc" "$svc"
done

write_env_prod_file
# Final status message
echo -e "${BLUE}Process has finished.${NC}"