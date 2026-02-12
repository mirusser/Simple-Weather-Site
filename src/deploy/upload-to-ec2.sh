#!/usr/bin/env bash
set -Eeuo pipefail

# upload-to-ec2.sh
#
# Uploads deployment files to an EC2 instance (/opt/sws) via scp.
#
# Usage:
#   ./upload-to-ec2.sh <EC2_PUBLIC_IP> [--dry-run] [--skip-env]
#
# Notes:
# - This script will create /opt/sws on the EC2 host (via sudo) and chown it to SSH_USER.
# - It will also run: chmod +x /opt/sws/deploy-ec2.sh remotely after upload.
# - If you want to avoid overwriting environment files (.env.*), use --skip-env.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

EC2_IP="${1:-}"
shift || true

if [[ -z "${EC2_IP}" || "${EC2_IP}" == -* ]]; then
  echo "Usage: $0 <EC2_PUBLIC_IP> [--dry-run] [--skip-env]"
  exit 1
fi

DRY_RUN=0
SKIP_ENV=0

for arg in "$@"; do
  case "$arg" in
    --dry-run) DRY_RUN=1 ;;
    --skip-env) SKIP_ENV=1 ;;
    *)
      echo "Unknown option: $arg"
      echo "Usage: $0 <EC2_PUBLIC_IP> [--dry-run] [--skip-env]"
      exit 1
      ;;
  esac
done

SSH_USER="${SSH_USER:-admin}"
KEY_PATH="${KEY_PATH:-/home/mirusser/sws-ec2-key-pair.pem}"
REMOTE_DIR="${REMOTE_DIR:-/opt/sws}"

BASE_FILES=(
  "docker-compose.prod.yml"
  "docker-compose.infra.prod.yml"
  "deploy-ec2.sh"
  "nginx.conf"
)

ENV_FILES=(
  ".env.prod"
  ".env.infra"
)

run() {
  if [[ "$DRY_RUN" -eq 1 ]]; then
    echo "[dry-run] $*"
  else
    "$@"
  fi
}

need_file() {
  local path="$1"
  if [[ ! -f "$path" ]]; then
    echo "ERROR: Missing local file: $path"
    exit 1
  fi
}

echo "Source dir: $SCRIPT_DIR"
echo "Target:     ${SSH_USER}@${EC2_IP}:${REMOTE_DIR}"
echo "Key:        ${KEY_PATH}"
echo "Mode:       $([[ "$DRY_RUN" -eq 1 ]] && echo "dry-run" || echo "live")"
echo "Env:        $([[ "$SKIP_ENV" -eq 1 ]] && echo "SKIP (.env.* will NOT be uploaded)" || echo "UPLOAD (.env.* will be uploaded)")"
echo

# Ensure local files exist (relative to SCRIPT_DIR)
for f in "${BASE_FILES[@]}"; do need_file "$SCRIPT_DIR/$f"; done
if [[ "$SKIP_ENV" -eq 0 ]]; then
  for f in "${ENV_FILES[@]}"; do need_file "$SCRIPT_DIR/$f"; done
fi

# Ensure remote dir exists and is writable by SSH_USER
run ssh -i "$KEY_PATH" "${SSH_USER}@${EC2_IP}" \
  "sudo mkdir -p '$REMOTE_DIR' && sudo chown -R '$SSH_USER':'$SSH_USER' '$REMOTE_DIR'"

# Upload base files
for f in "${BASE_FILES[@]}"; do
  echo "-> scp $f"
  run scp -i "$KEY_PATH" "$SCRIPT_DIR/$f" "${SSH_USER}@${EC2_IP}:${REMOTE_DIR}/"
done

# Upload env files unless skipped
if [[ "$SKIP_ENV" -eq 0 ]]; then
  for f in "${ENV_FILES[@]}"; do
    echo "-> scp $f"
    run scp -i "$KEY_PATH" "$SCRIPT_DIR/$f" "${SSH_USER}@${EC2_IP}:${REMOTE_DIR}/"
  done
else
  echo "-> skipping .env.prod and .env.infra"
fi

# Remote chmod +x deploy-ec2.sh
run ssh -i "$KEY_PATH" "${SSH_USER}@${EC2_IP}" \
  "chmod +x '${REMOTE_DIR}/deploy-ec2.sh'"

echo
echo "Done."
echo
echo "Next steps on EC2:"
echo "  ssh -i \"$KEY_PATH\" ${SSH_USER}@${EC2_IP}"
echo "  cd ${REMOTE_DIR}"
echo "  ./deploy-ec2.sh"