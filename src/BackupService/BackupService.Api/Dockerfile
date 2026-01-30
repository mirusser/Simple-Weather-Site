# Use the appropriate ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Copy the compiled app to the container
COPY deploy .

# Set environment variables
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT Docker

EXPOSE 80

ENTRYPOINT ["dotnet", "BackupService.Api.dll"]