#!/bin/bash

# There are so many comments here, 
# cause I'm not really familiar with bash scripting
# and I'm not writing it often - mea culpa

# This script is meant to run locally for dev purposes
# Run with sudo (in folder with file), use this command:
# sudo ./publish.sh

# Colors for better readability
GREEN='\033[0;32m' # for success messages
YELLOW='\033[0;33m' # for warning messages
RED='\033[0;31m' # for error messages
BLUE='\033[0;34m' # for custom informative messages
NC='\033[0m' # No Color - for default/informative messages

# Defines where solution and its docker-compose.yml files are located
BASE_DIR="${1:-/home/arnie/Repos/Projects/Simple-Weather-Site/src}"

check_dependencies() {

    # Make sure that iptables,iptables-persistent, netstat packages are installed, if not run:
    # sudo apt-get update
    # sudo apt-get install iptables
    # sudo apt-get install net-tools

    # Make sure that iptables is added to path
    # echo $PATH
    # if not then add it (add the end of the file):
    # nano ~/.bashrc
    # export PATH=$PATH:/usr/sbin

    local dependencies=("docker" "netstat" "iptables")
    for dep in "${dependencies[@]}"; do
        if ! command -v $dep &> /dev/null; then
            echo "$dep could not be found. Please install $dep."
            exit 1
        fi
    done
}

create_network(){

    # Network used by docker containers
    local network_name="overlaynetwork"

    # Check if the network already exists
    if ! docker network ls --format "{{.Name}}" | grep -wq $network_name; then
        # Create the Docker network since it doesn't exist
        docker network create -d bridge "$network_name"
        echo -e "${GREEN}Network $network_name created.${NC}"
    else
        echo -e "${BLUE}Network $network_name already exists.${NC}"
    fi
}

build_and_dockerize() {

    local project_local_dir="$1"
    local docker_image_name="$2"

    local project_dir="$BASE_DIR/$project_local_dir"
    local publish_dir="$project_dir/deploy"

    # Clean up the previous publish directory and create a new one
    rm -rf "$publish_dir"
    mkdir -p "$publish_dir"

    # Navigate to the project directory or exit if it fails
    cd "$project_dir" || { echo -e "${RED}Failed to navigate to $project_dir.${NC}"; exit 1; }

    # Publish the .NET project and build the Docker image
    dotnet publish -c Release --property:PublishDir="$publish_dir" && \
    docker build -t "$docker_image_name" . || \
    { echo -e "${RED}Build or Docker build failed for $docker_image_name.${NC}"; exit 1; }
}

build_and_run_docker_compose(){

   docker compose build && docker compose -p "sws-containers" up -d && \
    echo -e "${GREEN}=> The containers started in the background.${NC}" || \
    { echo -e "${RED}=> Docker compose failed.${NC}"; exit 1; }
}

forward_ports(){

    # Define an array of ports to check
    ports=(5672 27017 6379 5432 5341 1435 1433 9200 9300)
    
    # Loop through each port in the array
    for PORT in "${ports[@]}"; do
        echo "Checking port: $PORT"
        
        # Check if the port is already allowed in the iptables firewall
        if ! iptables -C INPUT -p tcp --dport $PORT -j ACCEPT 2>/dev/null; then
            echo -e "${YELLOW}Port $PORT is not allowed in iptables. Adding rule to allow it.${NC}"
            iptables -I INPUT -p tcp --dport $PORT -j ACCEPT

            # Save the iptables rule (uncomment the line corresponding to your system)
            iptables-save > /etc/iptables/rules.v4 # Debian/Ubuntu
            # service iptables save # CentOS/RHEL
            # systemctl restart iptables # If your system uses systemd and iptables
        else
            echo -e "${BLUE}Port $PORT is already allowed in iptables.${NC}"
        fi
    
        # Check if the port is listening
        if ! netstat -tuln | grep -q ":$PORT"; then
            echo -e "${RED}Port $PORT is not listening. You might need to start the service that uses this port.${NC}"
        else
            echo -e "${GREEN}Port $PORT is listening.${NC}"
        fi
    done
}

check_dependencies

create_network

build_and_dockerize "/Authorization/OAuthServer" "oauthserver"
build_and_dockerize "/CitiesService/CitiesService.Api" "citiesservice"
build_and_dockerize "/CitiesService/CitiesGrpcService" "citiesgrpcservice"
build_and_dockerize "/WeatherService" "weatherservice"
build_and_dockerize "/WeatherHistoryService" "weatherhistoryservice"
build_and_dockerize "/EmailService/EmailService.Api" "emailservice"
build_and_dockerize "/IconService/IconService.Api" "iconservice"
build_and_dockerize "/SignalRServer" "signalrserver"
build_and_dockerize "/HangfireService" "hangfireservice"
build_and_dockerize "/WeatherSite/Site" "weathersite"

build_and_run_docker_compose

forward_ports

# Wait for user input to continue
read -p "Press enter to continue . . ."

