using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Application.Features.City.Queries.GetCitiesPagination;
using CitiesService.Domain.Entities;
using CitiesService.Infrastructure.Repositories;
using CitiesService.Tests.TestDoubles;
using CitiesService.Tests.TestInfrastructure;

namespace CitiesService.Tests.Features.City.Queries;

public class GetCitiesPaginationHandlerTests
{
    [Fact]
    public async Task Handle_WhenCacheHit_MapsFromCachedPaginationDto_AndDoesNotSetCache()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repo = new GenericRepository<CityInfo>(db);
        var cache = new FakeCacheManager();

        var cached = new CityInfoPaginationDto
        {
            NumberOfAllCities = 123,
            CityInfos =
            [
                new CityInfo
                {
                    Id = 1,
                    CityId = 100m,
                    Name = "Alpha",
                    CountryCode = "AA",
                    Lat = 1,
                    Lon = 2,
                    State = null
                }
            ]
        };

        cache.TryGetOverride = _ => (true, cached);
        var sut = new GetCitiesPaginationHandler(repo, cache);

        var query = new GetCitiesPaginationQuery { NumberOfCities = 10, PageNumber = 1 };
        var result = await sut.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(123, result.Value!.NumberOfAllCities);
        Assert.Single(result.Value.Cities);
        Assert.Equal("Alpha", result.Value.Cities[0].Name);
        Assert.Equal(0, cache.SetCallCount);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ReturnsTotalCount_AndSetsCache()
    {
        await using var db = DbContextFactory.CreateInMemory();
        db.CityInfos.AddRange(
            new CityInfo { Id = 1, CityId = 1m, Name = "A", CountryCode = "X", Lat = 0, Lon = 0 },
            new CityInfo { Id = 2, CityId = 2m, Name = "B", CountryCode = "X", Lat = 0, Lon = 0 },
            new CityInfo { Id = 3, CityId = 3m, Name = "C", CountryCode = "X", Lat = 0, Lon = 0 });
        await db.SaveChangesAsync();

        var repo = new GenericRepository<CityInfo>(db);
        var cache = new FakeCacheManager();
        var sut = new GetCitiesPaginationHandler(repo, cache);

        var query = new GetCitiesPaginationQuery { NumberOfCities = 2, PageNumber = 1 };
        var result = await sut.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value!.NumberOfAllCities);
        Assert.Equal(2, result.Value.Cities.Count);
        Assert.Equal(1, cache.SetCallCount);
        Assert.NotNull(cache.LastSetKey);
        Assert.Contains("GetCitiesPagination-", cache.LastSetKey);
    }

    [Fact]
    public async Task Handle_PaginatesAndOrdersByName()
    {
        await using var db = DbContextFactory.CreateInMemory();
        db.CityInfos.AddRange(
            new CityInfo { Id = 1, CityId = 1m, Name = "Delta", CountryCode = "X", Lat = 0, Lon = 0 },
            new CityInfo { Id = 2, CityId = 2m, Name = "Bravo", CountryCode = "X", Lat = 0, Lon = 0 },
            new CityInfo { Id = 3, CityId = 3m, Name = "Echo", CountryCode = "X", Lat = 0, Lon = 0 },
            new CityInfo { Id = 4, CityId = 4m, Name = "Alpha", CountryCode = "X", Lat = 0, Lon = 0 },
            new CityInfo { Id = 5, CityId = 5m, Name = "Charlie", CountryCode = "X", Lat = 0, Lon = 0 });
        await db.SaveChangesAsync();

        var repo = new GenericRepository<CityInfo>(db);
        var cache = new FakeCacheManager();
        var sut = new GetCitiesPaginationHandler(repo, cache);

        // Ordered by Name: Alpha, Bravo, Charlie, Delta, Echo
        // Page 2 (2 per page) => Charlie, Delta
        var query = new GetCitiesPaginationQuery { NumberOfCities = 2, PageNumber = 2 };
        var result = await sut.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Cities.Count);
        Assert.Equal(new[] { "Charlie", "Delta" }, result.Value.Cities.Select(c => c.Name).ToArray());
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(10, 0)]
    public async Task Handle_WhenInputsAreInvalid_ReturnsCountButNoCities(int numberOfCities, int pageNumber)
    {
        await using var db = DbContextFactory.CreateInMemory();
        db.CityInfos.Add(new CityInfo { Id = 1, CityId = 1m, Name = "A", CountryCode = "X", Lat = 0, Lon = 0 });
        await db.SaveChangesAsync();

        var repo = new GenericRepository<CityInfo>(db);
        var cache = new FakeCacheManager();
        var sut = new GetCitiesPaginationHandler(repo, cache);

        var query = new GetCitiesPaginationQuery { NumberOfCities = numberOfCities, PageNumber = pageNumber };
        var result = await sut.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value!.NumberOfAllCities);
        Assert.Empty(result.Value.Cities);
    }
}
