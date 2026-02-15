using System.Linq.Expressions;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Commands.AddCitiesToDatabase;
using CitiesService.Application.Features.City.Services;
using CitiesService.Domain.Entities;
using Moq;

namespace CitiesService.Tests.Features.City.Commands;

public class AddCitiesToDatabaseHandlerTests
{
    [Fact]
    public async Task Handle_WhenCitiesExist_ReturnsConflict_AndDoesNotSeed()
    {
        var repo = new Mock<IGenericRepository<CityInfo>>(MockBehavior.Strict);
        repo.Setup(r => r.CheckIfExistsAsync(
                It.IsAny<Expression<Func<CityInfo, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var seeder = new Mock<ICitiesSeeder>(MockBehavior.Strict);

        var sut = new AddCitiesToDatabaseHandler(repo.Object, seeder.Object);

        var result = await sut.Handle(new AddCitiesToDatabaseCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Problem);
        Assert.Equal(409, result.Problem?.Status);
        Assert.Equal("Cities already added", result.Problem?.Message);

        seeder.Verify(s => s.SeedIfEmptyAsync(It.IsAny<CancellationToken>()), Times.Never);
        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenNoCitiesExist_Seeds_AndReturnsOk()
    {
        var repo = new Mock<IGenericRepository<CityInfo>>(MockBehavior.Strict);
        repo.Setup(r => r.CheckIfExistsAsync(
                It.IsAny<Expression<Func<CityInfo, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var seeder = new Mock<ICitiesSeeder>(MockBehavior.Strict);
        seeder.Setup(s => s.SeedIfEmptyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new AddCitiesToDatabaseHandler(repo.Object, seeder.Object);

        var result = await sut.Handle(new AddCitiesToDatabaseCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value!.IsSuccess);
        Assert.False(result.Value.IsAlreadyAdded);

        seeder.Verify(s => s.SeedIfEmptyAsync(It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenSeederReturnsFalse_ReturnsOkWithIsSuccessFalse()
    {
        var repo = new Mock<IGenericRepository<CityInfo>>(MockBehavior.Strict);
        repo.Setup(r => r.CheckIfExistsAsync(
                It.IsAny<Expression<Func<CityInfo, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var seeder = new Mock<ICitiesSeeder>(MockBehavior.Strict);
        seeder.Setup(s => s.SeedIfEmptyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = new AddCitiesToDatabaseHandler(repo.Object, seeder.Object);

        var result = await sut.Handle(new AddCitiesToDatabaseCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(result.Value!.IsSuccess);
        Assert.False(result.Value.IsAlreadyAdded);

        seeder.Verify(s => s.SeedIfEmptyAsync(It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyAll();
    }
}
