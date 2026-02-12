#!/bin/bash

# This script is meant to run locally for dev purposes
# Run with sudo (in folder with file), use this command:
# > sudo ./publish.sh

# To setup for https in docker reference file:
# AdditionalInfo/HttpsInDocker.md
# (Make sure to setup it up before running this script)

# Colors for better readability
GREEN='\033[0;32m' # for success messages
YELLOW='\033[0;33m' # for warning messages
RED='\033[0;31m' # for error messages
BLUE='\033[0;34m' # for custom informative messages
NC='\033[0m' # No Color - for default/informative messages

# strict mode + better error handling
# -E  : functions inherit ERR trap
# -e  : exit immediately on error (non-zero exit code)
# -u  : treat unset variables as an error
# -o pipefail : fail a pipeline if any command in it fails
set -Eeuo pipefail
# Print a helpful message when something fails (line number + failing command)
trap 'echo -e "${RED}Error on line $LINENO: $BASH_COMMAND${NC}"' ERR


SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# script will be in: <repo>/src/deploy
SRC_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"       # <repo>/src
DEPLOY_DIR="$SCRIPT_DIR"                      # <repo>/src/deploy

# Defines where solution and projects are located
# You can override this by passing a first argument:
#   ./publish.sh /path/to/src
BASE_DIR="${1:-$SRC_DIR}"

# Defines where solution and its docker-compose.yml files are located
# You can override this by passing a first argument:
#   ./publish.sh /path/to/src
# BASE_DIR="${1:-/home/mirusser/Repos/Simple-Weather-Site/src}"

# Explicitly configure framework to publish .NET projects
# You can override this via env var:
#   FRAMEWORK=net10.0 ./publish.sh
FRAMEWORK="${FRAMEWORK:-net10.0}"

check_dependencies() {

    # Make sure that iptables,iptables-persistent, netstat packages are installed, if not run:
    # > sudo apt-get update
    # > sudo apt-get update # optional, but maybe it's worth to upgrade
    # > sudo apt-get install iptables
    # > sudo apt-get install net-tools

    # ss comes from iproute2 (usually installed by default):
    # > sudo apt-get install -y iproute2

    # install .net sdk (version may change in the future)
    # on debain based linux distros:
    # > sudo apt update
    # > sudo apt install -y wget apt-transport-https ca-certificates gnupg
    # > wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb
    # > sudo dpkg -i packages-microsoft-prod.deb
    # > rm packages-microsoft-prod.deb
    # > sudo apt update
    # > sudo apt install dotnet-sdk-10.0
    # If it doesn’t find dotnet-sdk-10.0, list available versions:
    # > apt search dotnet-sdk
    # Verify:
    # > dotnet --info
    # > dotnet --list-sdks

    # Make sure that iptables is added to path
    # echo $PATH
    # if not then add it (add the end of the file):
    # nano ~/.bashrc
    # export PATH=$PATH:/usr/sbin

    # NOTE:
    # We check only "user-level" commands here (no sudo required):
    # - docker   : build images / run compose
    # - netstat  : optional legacy tool (net-tools). Script doesn't rely on it heavily, but it's checked.
    # - ss       : modern socket inspection tool (used in check_port_binding)
    local dependencies=("docker" "netstat" "ss")
    for dep in "${dependencies[@]}"; do
        if ! command -v "$dep" &> /dev/null; then
            echo "$dep could not be found. Please install $dep."
            exit 1
        fi
    done

    # iptables is usually in /sbin and requires root anyway
    # NOTE:
    # -n tells sudo "non-interactive" (don't prompt). We only use it to *detect* whether a prompt will happen later.
    # This does not fail the script; it only prints a friendly heads-up.
    if ! sudo -n true 2>/dev/null; then
        echo -e "${YELLOW}Sudo access required for iptables rules. You may be prompted later.${NC}"
    fi

    # Verify that iptables exists (with sudo), because it's commonly under /sbin or /usr/sbin
    # and may not be visible in a normal user's PATH.
    # If iptables is missing, later firewall steps will fail anyway.
    if ! sudo command -v iptables &>/dev/null; then
        if ! sudo test -x /usr/sbin/iptables && ! sudo test -x /sbin/iptables; then
        echo -e "${RED}iptables could not be found (even with sudo). Please install iptables.${NC}"
        exit 1
        fi
    fi

    # Ensure docker daemon is reachable
    if ! docker info &> /dev/null; then
        echo -e "${RED}Docker daemon not reachable. Is docker running? Do you have permission to access it?${NC}"
        exit 1
    fi

    # Ensure docker compose is available (plugin)
    if ! docker compose version &> /dev/null; then
        echo -e "${RED}docker compose not available. Please install docker-compose-plugin or a compatible Docker version.${NC}"
        exit 1
    fi
}

create_network(){

    # Shared user-defined bridge network for app + infra compose stacks
    local network_name="sws-containers-bridge-network"

    # Check if the network already exists
    # NOTE: -F = fixed string, -x = match whole line, -q = quiet
    # This avoids false positives from partial matches.
    if ! docker network ls --format "{{.Name}}" | grep -Fxq "$network_name"; then
        # Create the Docker network since it doesn't exist
        # NOTE: This is a user-defined "bridge" network (not a Swarm overlay network).
        docker network create -d bridge "$network_name"
        echo -e "${GREEN}Network $network_name created.${NC}"
    else
        echo -e "${BLUE}Network $network_name already exists.${NC}"
    fi
}

build_docker_image() {

    # Arguments:
    local project_local_dir="$1" # $1 = relative path to the project from BASE_DIR (e.g. "/CitiesService/CitiesService.Api")
    local docker_image_name="$2" # $2 = docker image name/tag to build (e.g. "citiesservice")

    local project_dir="$BASE_DIR/$project_local_dir"

    cd "$BASE_DIR" || { echo -e "${RED}Failed to navigate to $BASE_DIR.${NC}"; exit 1; }

    docker build -t "$docker_image_name" -f "$project_dir/Dockerfile" "$BASE_DIR" || \
    { echo -e "${RED}Docker build failed for $docker_image_name.${NC}"; exit 1; }
}

run_docker_compose(){

    # Start containers in detached mode (-d)
    # -p sets an explicit compose project name so container naming is predictable
    #docker compose -p "sws-containers" up -d && \
    docker compose \
        --project-directory "$BASE_DIR" \
        -f "$DEPLOY_DIR/docker-compose.local.yml" \
        -p "sws-containers" up -d && \
    echo -e "${GREEN}=> The containers started in the background.${NC}" || \
    { echo -e "${RED}=> Docker compose failed.${NC}"; exit 1; }
}

update_iptables() {
    local port=$1

    # NOTE:
    # This opens the port in the host firewall (INPUT chain) so services are reachable from outside
    # the machine (e.g., LAN clients). If you only need local machine access, you might not need this.
    # -C checks whether the rule already exists (idempotent)
    if ! sudo iptables -C INPUT -p tcp --dport $port -j ACCEPT 2>/dev/null; then
        echo -e "${YELLOW}Port $port is not allowed in iptables. Adding rule to allow it.${NC}"
        # Insert rule at the top so it takes effect before potential DROP rules later in the chain
        sudo iptables -I INPUT -p tcp --dport $port -j ACCEPT
    else
        echo -e "${BLUE}Port $port is already allowed in iptables.${NC}"
    fi
}

check_port_binding() {
    local port=$1

    # Check if the port is listening and capture the output of ss related to this port
    # Tight match to avoid false positives (e.g. 80 matching 8080)
    # NOTE:
    # ss output contains local address:port in column 5; awk filters exact-ish matches for ":$port"
    local listening_info="$(ss -tulnH | awk -v p=":$port" '$5 ~ p"($|[^0-9])" {print}')"

    if [ -z "$listening_info" ]; then
        echo -e "${RED}Port $port is not listening. You might need to start the service that uses this port.${NC}"
    else
        echo -e "${GREEN}Port $port is listening.${NC}"
        # Check if the service is bound to localhost or all interfaces
        # NOTE:
        # - 127.0.0.1 means only local access (no external clients)
        # - 0.0.0.0 means all interfaces (external clients can reach it if firewall allows)
        if echo "$listening_info" | grep -q "127.0.0.1"; then
            echo -e "${YELLOW}Port $port is bound to localhost only.${NC}"
        elif echo "$listening_info" | grep -q "0.0.0.0"; then
            echo -e "${GREEN}Port $port is bound to all interfaces (0.0.0.0).${NC}"
        else
            echo -e "${BLUE}Port $port is bound to a specific external interface.${NC}"
        fi
    fi
}

forward_ports() {
    # Ports that should be reachable from outside the host (LAN/other machines)
    local ports=(5672 27017 6379 5432 5341 1435 1433 9200 9300 8079 8078 8181 8081 8090 8897)
    for port in "${ports[@]}"; do
        echo "Checking port: $port"
        # Ensure firewall allows it (adds rule if missing)
        update_iptables $port
        # Print whether something is listening and where it's bound
        check_port_binding $port
    done

    # Save iptables rules once after all updates (uncomment the line corresponding to your system)
    # NOTE:
    # This persists current rules across reboot when using iptables-persistent on Debian.
    sudo sh -c 'iptables-save > /etc/iptables/rules.v4'
    # service iptables save # CentOS/RHEL

    # NOTE: Do NOT restart iptables here.
    # Rules are applied immediately; restart may wipe Docker rules or drop SSH.
    # Restart only if its absolutely neccesery
    # still it might be safer to use: iptables-restore < /etc/iptables/rules.v4
    # systemctl restart iptables # If your system uses systemd and iptables
}

# ---- Script entrypoint (runs in order) ----

# Validate tools and environment
check_dependencies

# Ensure the shared external docker network exists
create_network

# Build each service docker image and tag images locally
# NOTE: Each build uses the project's Dockerfile from its own directory.
build_docker_image "/Authorization/OAuthServer" "oauthserver"
build_docker_image "/CitiesService/CitiesService.Api" "citiesservice"
build_docker_image "/CitiesService/CitiesGrpcService" "citiesgrpcservice"
build_docker_image "/WeatherService" "weatherservice"
build_docker_image "/WeatherHistoryService" "weatherhistoryservice"
build_docker_image "/EmailService/EmailService.Api" "emailservice"
build_docker_image "/IconService/IconService.Api" "iconservice"
build_docker_image "/SignalRServer" "signalrserver"
build_docker_image "/HangfireService" "hangfireservice"
build_docker_image "/BackupService/BackupService.Api" "backupservice"
build_docker_image "/WeatherSite/Site" "weathersite"

# Bring the docker-compose stack up
run_docker_compose

# Open host firewall ports and persist rules
forward_ports

# Final status message
echo -e "${BLUE}Process has finished.${NC}"