# Use the appropriate ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Copy the compiled app to the container
COPY deploy .

# Copy the .pfx certificate into the container
COPY cert/localhost.pfx /https/localhost.pfx

# Set environment variables
ENV ASPNETCORE_URLS="https://+:443;http://+:80"
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="zaq1@WSX"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.pfx

EXPOSE 80 443

ENTRYPOINT ["dotnet", "CitiesService.Api.dll"]
