# Build stage

# Use the appropriate.NET sdk image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire src/ directory (build context should be src/)
COPY . .

# Publish only this project (includes referenced projects via ProjectReference)
RUN dotnet publish -c Release -f net10.0 \
WeatherService/WeatherService.csproj \
-o /app/publish

# Runtime stage

# Use the appropriate ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS="http://+:80"
ENV ASPNETCORE_ENVIRONMENT="Docker"

EXPOSE 80

ENTRYPOINT ["dotnet", "WeatherService.dll"]