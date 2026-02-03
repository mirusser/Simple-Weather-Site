# Build stage

# Use the appropriate.NET sdk image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire /src directory (build context should be src/)
COPY . .

# Publish only this project (includes referenced projects via ProjectReference)
RUN dotnet publish -c Release -f net10.0 \
    Authorization/OAuthServer/OAuthServer.csproj \
    -o /app/publish

# Runtime stage

# Use the appropriate.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Copy the .pfx certificate into the container
COPY Authorization/OAuthServer/localhost.pfx /https/localhost.pfx

# Set environment variables
ENV ASPNETCORE_URLS="https://+:443;http://+:80"
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="zaq1@WSX"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.pfx

EXPOSE 80 443

ENTRYPOINT ["dotnet", "OAuthServer.dll"]