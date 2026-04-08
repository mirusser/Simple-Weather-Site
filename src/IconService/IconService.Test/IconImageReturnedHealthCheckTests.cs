using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Mediator;
using IconService.Application.Icon.HealthChecks;
using IconService.Application.Icon.Models.Dto;
using IconService.Application.Icon.Queries.Get;
using IconService.Domain.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IconService.Test;

public class IconImageReturnedHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenConfiguredIconReturnsImage_ReturnsHealthy()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.SendAsync(
                It.Is<GetQuery>(q => q.Icon == "03d"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetResult
            {
                Icon = "03d",
                FileContent = [1, 2, 3],
                Id = Guid.NewGuid().ToString(),
                Name = "03d@2x.png",
                Description = "scattered clouds"
            });

        var sut = new IconImageReturnedHealthCheck(
            mediator.Object,
            Options.Create(new ServiceSettings { IconsPath = "//app//Icons", HealthCheckIcon = "03d" }),
            NullLogger<IconImageReturnedHealthCheck>.Instance);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Healthy, result.Status);
        mediator.VerifyAll();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenPayloadIsEmpty_ReturnsUnhealthy()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.SendAsync(
                It.IsAny<GetQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetResult
            {
                Icon = "03d",
                FileContent = [],
                Id = Guid.NewGuid().ToString(),
                Name = "03d@2x.png",
                Description = "scattered clouds"
            });

        var sut = new IconImageReturnedHealthCheck(
            mediator.Object,
            Options.Create(new ServiceSettings { IconsPath = "//app//Icons", HealthCheckIcon = "03d" }),
            NullLogger<IconImageReturnedHealthCheck>.Instance);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        mediator.VerifyAll();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenReturnedIconDoesNotMatchConfiguredIcon_ReturnsUnhealthy()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.SendAsync(
                It.IsAny<GetQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetResult
            {
                Icon = "04d",
                FileContent = [1, 2, 3],
                Id = Guid.NewGuid().ToString(),
                Name = "04d@2x.png",
                Description = "broken clouds"
            });

        var sut = new IconImageReturnedHealthCheck(
            mediator.Object,
            Options.Create(new ServiceSettings { IconsPath = "//app//Icons", HealthCheckIcon = "03d" }),
            NullLogger<IconImageReturnedHealthCheck>.Instance);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        mediator.VerifyAll();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenQueryThrows_ReturnsUnhealthy()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.SendAsync(
                It.IsAny<GetQuery>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var sut = new IconImageReturnedHealthCheck(
            mediator.Object,
            Options.Create(new ServiceSettings { IconsPath = "//app//Icons", HealthCheckIcon = "03d" }),
            NullLogger<IconImageReturnedHealthCheck>.Instance);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        mediator.VerifyAll();
    }

    [Fact]
    public void ServiceSettings_WhenHealthCheckIconIsMissing_UsesDefault()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ServiceSettings:IconsPath"] = "//app//Icons"
            })
            .Build();

        var settings = new ServiceSettings();
        configuration.GetSection(nameof(ServiceSettings)).Bind(settings);

        Assert.Equal("03d", settings.HealthCheckIcon);
    }

    [Fact]
    public void ServiceSettings_WhenHealthCheckIconIsConfigured_OverridesDefault()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ServiceSettings:IconsPath"] = "//app//Icons",
                ["ServiceSettings:HealthCheckIcon"] = "11d"
            })
            .Build();

        var settings = new ServiceSettings();
        configuration.GetSection(nameof(ServiceSettings)).Bind(settings);

        Assert.Equal("11d", settings.HealthCheckIcon);
    }
}
