FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY deploy .
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT Docker
ENTRYPOINT ["dotnet", "WeatherService.dll"]