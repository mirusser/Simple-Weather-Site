using System.Net;
using System.Text;
using Common.Infrastructure.Managers.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using WeatherService.Clients;
using WeatherService.HealthChecks;
using WeatherService.Settings;
using Xunit;

namespace WeatherService.Tests;

public class OpenWeatherHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenWeatherClientReturnsForecast_ReturnsHealthy()
    {
        var httpExecutor = new Mock<IHttpExecutor>(MockBehavior.Strict);
        httpExecutor.Setup(e => e.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("id=756135")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateJsonResponse(HttpStatusCode.OK, """
                {
                  "coord": { "lon": 21.0118, "lat": 52.2298 },
                  "weather": [{ "id": 803, "main": "Clouds", "description": "broken clouds", "icon": "04d" }],
                  "main": {
                    "temp": 7,
                    "feels_like": 5,
                    "temp_min": 6,
                    "temp_max": 8,
                    "pressure": 1012,
                    "humidity": 75
                  },
                  "visibility": 10000,
                  "wind": { "speed": 2.5, "deg": 220 },
                  "clouds": { "all": 75 },
                  "dt": 1712592000,
                  "sys": { "type": 2, "id": 2000001, "country": "PL", "sunrise": 1712541600, "sunset": 1712590200 },
                  "timezone": 7200,
                  "id": 756135,
                  "name": "Warsaw",
                  "cod": 200
                }
                """));

        var sut = new OpenWeatherHealthCheck(
            new WeatherClient(
                httpExecutor.Object,
                Options.Create(new ServiceSettings
                {
                    OpenWeatherHost = "https://api.openweathermap.org/",
                    ApiKey = "test-key",
                    HealthCheckCityId = 756135
                })),
            Options.Create(new ServiceSettings
            {
                OpenWeatherHost = "https://api.openweathermap.org/",
                ApiKey = "test-key",
                HealthCheckCityId = 756135
            }),
            NullLogger<OpenWeatherHealthCheck>.Instance);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Healthy, result.Status);
        httpExecutor.VerifyAll();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenUpstreamReturnsUnauthorized_ReturnsUnhealthy()
    {
        var httpExecutor = new Mock<IHttpExecutor>(MockBehavior.Strict);
        httpExecutor.Setup(e => e.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<HttpRequestMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateJsonResponse(HttpStatusCode.Unauthorized, "{}"));

        var sut = CreateSut(httpExecutor.Object, 756135);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        httpExecutor.VerifyAll();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenPayloadIsIncomplete_ReturnsUnhealthy()
    {
        var httpExecutor = new Mock<IHttpExecutor>(MockBehavior.Strict);
        httpExecutor.Setup(e => e.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<HttpRequestMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateJsonResponse(HttpStatusCode.OK, """
                {
                  "coord": { "lon": 21.0118, "lat": 52.2298 },
                  "weather": [],
                  "main": {
                    "temp": 7,
                    "feels_like": 5,
                    "temp_min": 6,
                    "temp_max": 8,
                    "pressure": 1012,
                    "humidity": 75
                  },
                  "visibility": 10000,
                  "wind": { "speed": 2.5, "deg": 220 },
                  "clouds": { "all": 75 },
                  "dt": 1712592000,
                  "sys": { "type": 2, "id": 2000001, "country": "PL", "sunrise": 1712541600, "sunset": 1712590200 },
                  "timezone": 7200,
                  "id": 0,
                  "name": "",
                  "cod": 200
                }
                """));

        var sut = CreateSut(httpExecutor.Object, 756135);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        httpExecutor.VerifyAll();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenWeatherClientThrows_ReturnsUnhealthy()
    {
        var httpExecutor = new Mock<IHttpExecutor>(MockBehavior.Strict);
        httpExecutor.Setup(e => e.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<HttpRequestMessage>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var sut = CreateSut(httpExecutor.Object, 756135);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        httpExecutor.VerifyAll();
    }

    [Fact]
    public void ServiceSettings_WhenHealthCheckCityIdIsMissing_UsesDefault()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ServiceSettings:OpenWeatherHost"] = "https://api.openweathermap.org/",
                ["ServiceSettings:ApiKey"] = "test-key"
            })
            .Build();

        var settings = new ServiceSettings();
        configuration.GetSection(nameof(ServiceSettings)).Bind(settings);

        Assert.Equal(756135, settings.HealthCheckCityId);
    }

    [Fact]
    public void ServiceSettings_WhenHealthCheckCityIdIsConfigured_OverridesDefault()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ServiceSettings:OpenWeatherHost"] = "https://api.openweathermap.org/",
                ["ServiceSettings:ApiKey"] = "test-key",
                ["ServiceSettings:HealthCheckCityId"] = "3088171"
            })
            .Build();

        var settings = new ServiceSettings();
        configuration.GetSection(nameof(ServiceSettings)).Bind(settings);

        Assert.Equal(3088171, settings.HealthCheckCityId);
    }

    private static OpenWeatherHealthCheck CreateSut(IHttpExecutor httpExecutor, int cityId)
    {
        var serviceSettings = new ServiceSettings
        {
            OpenWeatherHost = "https://api.openweathermap.org/",
            ApiKey = "test-key",
            HealthCheckCityId = cityId
        };

        return new OpenWeatherHealthCheck(
            new WeatherClient(httpExecutor, Options.Create(serviceSettings)),
            Options.Create(serviceSettings),
            NullLogger<OpenWeatherHealthCheck>.Instance);
    }

    private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }
}
