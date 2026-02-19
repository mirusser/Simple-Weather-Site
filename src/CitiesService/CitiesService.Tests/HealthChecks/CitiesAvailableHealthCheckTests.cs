using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Application.Features.City.Queries.GetCitiesPagination;
using CitiesService.Application.Features.HealthChecks;
using Common.Mediator;
using Common.Presentation.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CitiesService.Tests.HealthChecks;

public class CitiesAvailableHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenAtLeastOneCityExists_ReturnsHealthy()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.SendAsync(
                It.IsAny<GetCitiesPaginationQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GetCitiesPaginationResult>.Ok(new GetCitiesPaginationResult
            {
                Cities = [new GetCityResult { Id = 1, Name = "A" }],
                NumberOfAllCities = 1
            }));

        var sut = new CitiesAvailableHealthCheck(mediator.Object, NullLogger<CitiesAvailableHealthCheck>.Instance);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Healthy, result.Status);
        mediator.VerifyAll();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenNoCities_ReturnsUnhealthy()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.SendAsync(
                It.IsAny<GetCitiesPaginationQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GetCitiesPaginationResult>.Ok(new GetCitiesPaginationResult
            {
                Cities = [],
                NumberOfAllCities = 0
            }));

        var sut = new CitiesAvailableHealthCheck(mediator.Object, NullLogger<CitiesAvailableHealthCheck>.Instance);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        mediator.VerifyAll();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenMediatorThrows_ReturnsUnhealthy()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.SendAsync(
                It.IsAny<GetCitiesPaginationQuery>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var sut = new CitiesAvailableHealthCheck(mediator.Object, NullLogger<CitiesAvailableHealthCheck>.Instance);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        mediator.VerifyAll();
    }
}
