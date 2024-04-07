#!/bin/bash

# This script is meant to run locally for dev purposes
# (Run with sudo)

# Colors for better readability
GREEN='\033[0;32m' # for success messages
YELLOW='\033[0;33m' # for warning messages
RED='\033[0;31m' # for error messages
BLUE='\033[0;34m' # for custom informative messages
NC='\033[0m' # No Color - for default/informative messages

# Network name
network_name="overlaynetwork"

# Check if the network already exists
if ! docker network ls --format "{{.Name}}" | grep -wq $network_name; then
    # Create the Docker network since it doesn't exist
    docker network create -d bridge "$network_name"
    echo -e "${GREEN}Network $network_name created.${NC}"
else
    echo -e "${BLUE}Network $network_name already exists.${NC}"
fi

# Define a base directory variable for easier path management
# Change path to reflect your local path 
BASE_DIR="/home/arnie/Repos/Projects/Simple-Weather-Site/src"

# CitiesGrpcService
rm -rf "$BASE_DIR/CitiesService/CitiesGrpcService/deploy"
mkdir -p "$BASE_DIR/CitiesService/CitiesGrpcService/deploy"
cd "$BASE_DIR/CitiesService/CitiesGrpcService" || exit
dotnet publish -c Release -o "$BASE_DIR/CitiesService/CitiesGrpcService/deploy"
docker build -t citiesgrpcservice .

# CitiesService
rm -rf "$BASE_DIR/CitiesService/CitiesService.Api/deploy"
mkdir -p "$BASE_DIR/CitiesService/CitiesService.Api/deploy"
cd "$BASE_DIR/CitiesService/CitiesService.Api" || exit
dotnet publish -c Release -o "$BASE_DIR/CitiesService/CitiesService.Api/deploy"
docker build -t citiesservice .

# WeatherService
rm -rf "$BASE_DIR/WeatherService/deploy"
mkdir -p "$BASE_DIR/WeatherService/deploy"
cd "$BASE_DIR/WeatherService" || exit
dotnet publish -c Release -o "$BASE_DIR/WeatherService/deploy"
docker build -t weatherservice .

# WeatherHistoryService
rm -rf "$BASE_DIR/WeatherHistoryService/deploy"
mkdir -p "$BASE_DIR/WeatherHistoryService/deploy"
cd "$BASE_DIR/WeatherHistoryService" || exit
dotnet publish -c Release -o "$BASE_DIR/WeatherHistoryService/deploy"
docker build -t weatherhistoryservice .

# EmailService
rm -rf "$BASE_DIR/EmailService/EmailService.Api/deploy"
mkdir -p "$BASE_DIR/EmailService/EmailService.Api/deploy"
cd "$BASE_DIR/EmailService/EmailService.Api" || exit
dotnet publish -c Release -o "$BASE_DIR/EmailService/EmailService.Api/deploy"
docker build -t emailservice .

# IconService
rm -rf "$BASE_DIR/IconService/IconService.Api/deploy"
mkdir -p "$BASE_DIR/IconService/IconService.Api/deploy"
cd "$BASE_DIR/IconService/IconService.Api" || exit
dotnet publish -c Release -o "$BASE_DIR/IconService/IconService.Api/deploy"
docker build -t iconservice .

# SignalRServer
rm -rf "$BASE_DIR/SignalRServer/deploy"
mkdir -p "$BASE_DIR/SignalRServer/deploy"
cd "$BASE_DIR/SignalRServer" || exit
dotnet publish -c Release -o "$BASE_DIR/SignalRServer/deploy"
docker build -t signalrserver .

# WeatherSite
rm -rf "$BASE_DIR/WeatherSite/Site/deploy"
mkdir -p "$BASE_DIR/WeatherSite/Site/deploy"
cd "$BASE_DIR/WeatherSite/Site" || exit
dotnet publish -c Release -o "$BASE_DIR/WeatherSite/Site/deploy"
docker build -t weathersite .

# Build and run with docker-compose
docker compose build
docker compose -p "sws-containers" up -d

echo -e "${GREEN}'Docker Compose' has started the containers in the background.${NC}"

# Make sure that iptables and netstat commands are installed, if not run:
# sudo apt-get update
# sudo apt-get install iptables
# sudo apt-get install net-tools

# Make sure that iptables is added to path
# echo $PATH
# if not then add it (add the end of the file):
# nano ~/.bashrc
# export PATH=$PATH:/usr/sbin


# TODO: maybe move the port listening to another script
# Define an array of ports you want to check
ports=(5672)

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

# Wait for user input to continue (mimics the pause command in Windows)
read -p "Press any key to continue . . . "

