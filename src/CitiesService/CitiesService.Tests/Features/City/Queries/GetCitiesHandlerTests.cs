using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Application.Features.City.Queries.GetCities;
using CitiesService.Domain.Entities;
using CitiesService.Infrastructure.Repositories;
using CitiesService.Tests.TestDoubles;
using CitiesService.Tests.TestInfrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace CitiesService.Tests.Features.City.Queries;

public class GetCitiesHandlerTests
{
    [Fact]
    public async Task Handle_TrimsCityName()
    {
        await using var db = DbContextFactory.CreateInMemory();
        db.CityInfos.Add(new CityInfo
        {
            Id = 1,
            CityId = 100m,
            Name = "London",
            CountryCode = "GB",
            Lat = 51.5074m,
            Lon = -0.1278m,
            State = null
        });
        await db.SaveChangesAsync();

        var repo = new GenericRepository<CityInfo>(db);
        var cache = new FakeCacheManager();
        var sut = new GetCitiesHandler(repo, cache, NullLogger<GetCitiesHandler>.Instance);

        var query = new GetCitiesQuery { CityName = "  Lon  ", Limit = 10 };

        var result = await sut.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Lon", query.CityName);
        Assert.NotNull(cache.LastTryGetKey);
        Assert.Contains("-Lon-", cache.LastTryGetKey);
    }

    [Fact]
    public async Task Handle_WhenLimitIsZero_ReturnsEmpty_AndDoesNotTouchCache()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repo = new GenericRepository<CityInfo>(db);
        var cache = new FakeCacheManager();
        var sut = new GetCitiesHandler(repo, cache, NullLogger<GetCitiesHandler>.Instance);

        var query = new GetCitiesQuery { CityName = "Lon", Limit = 0 };
        var result = await sut.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.Cities);
        Assert.Empty(result.Value.Cities!);
        Assert.Null(cache.LastTryGetKey);
        Assert.Equal(0, cache.SetCallCount);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ReturnsCachedValue_AndDoesNotSetCache()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repo = new GenericRepository<CityInfo>(db);
        var cache = new FakeCacheManager();

        var cached = new List<GetCityResult>
        {
            new()
            {
                Id = 1,
                Name = "Cached",
                Country = "XX",
                State = null,
                Coord = new Coord { Lat = 1, Lon = 2 }
            }
        };

        cache.TryGetOverride = _ => (true, cached);

        var sut = new GetCitiesHandler(repo, cache, NullLogger<GetCitiesHandler>.Instance);
        var query = new GetCitiesQuery { CityName = "Lon", Limit = 10 };

        var result = await sut.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Same(cached, result.Value!.Cities);
        Assert.Equal(0, cache.SetCallCount);
    }

    [Fact]
    public async Task Handle_WhenNoCitiesExist_ReturnsEmpty_AndDoesNotSetCache()
    {
        await using var db = DbContextFactory.CreateInMemory();
        db.CityInfos.Add(new CityInfo
        {
            Id = 1,
            CityId = 100m,
            Name = "Paris",
            CountryCode = "FR",
            Lat = 48.8566m,
            Lon = 2.3522m,
            State = null
        });
        await db.SaveChangesAsync();

        var repo = new GenericRepository<CityInfo>(db);
        var cache = new FakeCacheManager();
        var sut = new GetCitiesHandler(repo, cache, NullLogger<GetCitiesHandler>.Instance);

        var query = new GetCitiesQuery { CityName = "Lon", Limit = 10 };
        var result = await sut.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.Cities);
        Assert.Empty(result.Value.Cities!);
        Assert.Equal(0, cache.SetCallCount);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_DedupesByName_MapsFields_AndSetsCache()
    {
        await using var db = DbContextFactory.CreateInMemory();
        db.CityInfos.AddRange(
            new CityInfo
            {
                Id = 1,
                CityId = 101m,
                Name = "London",
                CountryCode = "GB",
                Lat = 51.5074m,
                Lon = -0.1278m,
                State = "England"
            },
            new CityInfo
            {
                Id = 2,
                CityId = 102m,
                Name = "London",
                CountryCode = "GB",
                Lat = 51.5000m,
                Lon = -0.1200m,
                State = "England"
            },
            new CityInfo
            {
                Id = 3,
                CityId = 201m,
                Name = "Londonderry",
                CountryCode = "GB",
                Lat = 54.9970m,
                Lon = -7.3090m,
                State = null
            });
        await db.SaveChangesAsync();

        var repo = new GenericRepository<CityInfo>(db);
        var cache = new FakeCacheManager();
        var sut = new GetCitiesHandler(repo, cache, NullLogger<GetCitiesHandler>.Instance);

        var query = new GetCitiesQuery { CityName = "Lon", Limit = 10 };
        var result = await sut.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.Cities);
        
        var cities = result.Value.Cities!.ToList();
        Assert.Equal(2, cities.Select(c => c.Name).Distinct().Count());
        Assert.Equal(2, cities.Count);

        var london = cities.Single(c => c.Name == "London");
        Assert.Equal("GB", london.Country);
        Assert.NotNull(london.Coord);

        Assert.Equal(1, cache.SetCallCount);
        Assert.NotNull(cache.LastSetKey);
        Assert.Contains("GetCitiesByName-", cache.LastSetKey);
    }
}
