#!/bin/bash

# This script is meant to run locally for dev purposes

# Create a Docker network
docker network create -d bridge overlaynetwork

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
docker compose -p "sws-containers" up

# Wait for user input to continue (mimics the pause command in Windows)
read -p "Press any key to continue . . . " -n1 -s