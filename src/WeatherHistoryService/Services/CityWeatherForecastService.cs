using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using WeatherHistoryService.Models.Dto;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService.Services;

public class CityWeatherForecastService(IMongoCollectionFactory<CityWeatherForecastDocument> mongoCollectionFactory)
    : ICityWeatherForecastService
{
    private readonly IMongoCollection<CityWeatherForecastDocument> cityWeatherForecastCollection
        = mongoCollectionFactory.Create();

    public async Task<IReadOnlyList<CityWeatherForecastDocument>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => await cityWeatherForecastCollection.Find(_ => true).ToListAsync(cancellationToken);

    public async Task<CityWeatherForecastPaginationDto> GetCityWeatherForecastPaginationAsync(
        int numberOfEntities = 25,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        CityWeatherForecastPaginationDto cityWeatherForecastPaginationDto = new();

        cityWeatherForecastPaginationDto.NumberOfAllEntities = (int)await cityWeatherForecastCollection
            .CountDocumentsAsync(c => c.Id != default, cancellationToken: cancellationToken);

        if (pageNumber >= 1 && numberOfEntities >= 1)
        {
            var howManyToSkip = pageNumber > 1
                ? numberOfEntities * (pageNumber - 1)
                : 0;

            var cityWeatherForecastDocuments = cityWeatherForecastCollection.AsQueryable()
                .Where(c => c.Id != default)
                .OrderByDescending(c => c.SearchDate)
                .Skip(howManyToSkip)
                .Take(numberOfEntities);

            cityWeatherForecastPaginationDto.WeatherForecastDocuments = await cityWeatherForecastDocuments
                .ToListAsync(cancellationToken);
        }

        return cityWeatherForecastPaginationDto;
    }

    public async Task<CityWeatherForecastDocument?> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        CityWeatherForecastDocument? cityWeatherForecastDocument = await cityWeatherForecastCollection
            .Find(c => c.Id == id)
            .SingleOrDefaultAsync(cancellationToken);

        return cityWeatherForecastDocument;
    }

    /// <summary>
    /// Inserts a new <see cref="CityWeatherForecastDocument"/> if it does not exist,
    /// or safely ignores the operation if the document has already been processed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is intentionally <b>idempotent</b>: repeated calls with the same
    /// <c>EventId</c> will not create duplicates and will not modify existing data.
    /// This is required to safely handle message retries, redelivery, and
    /// at-least-once delivery semantics used by the messaging infrastructure.
    /// </para>
    ///
    /// <para>
    /// Idempotency is enforced by:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     Using <c>EventId</c> as a deterministic, unique identifier for the event
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     Applying a MongoDB upsert (<c>IsUpsert = true</c>)
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     Writing fields exclusively via <c>SetOnInsert</c>, ensuring that once a
    ///     document is created, it is never mutated on subsequent retries
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <para>
    /// If a document with the given <c>EventId</c> already exists, this operation
    /// becomes a no-op. If it does not exist, the document is inserted atomically.
    /// </para>
    ///
    /// <para>
    /// <b>NOTE:</b> <c>EventId</c> must be provided; without it, idempotency cannot
    /// be guaranteed.
    /// </para>
    /// </remarks>
    public async Task<CityWeatherForecastDocument> UpsertIdempotentAsync(
        CityWeatherForecastDocument? cityWeatherForecast,
        CancellationToken cancellationToken = default)
    {
        cityWeatherForecast ??= new CityWeatherForecastDocument();

        // if EventId is missing, it can't be idempotent
        if (cityWeatherForecast.EventId == Guid.Empty)
        {
            throw new ArgumentException("EventId is required for idempotent insert.", nameof(cityWeatherForecast));
        }

        var filter = Builders<CityWeatherForecastDocument>.Filter
            .Eq(x => x.EventId, cityWeatherForecast.EventId);

        var update = Builders<CityWeatherForecastDocument>.Update
            .SetOnInsert(x => x.EventId, cityWeatherForecast.EventId)
            .SetOnInsert(x => x.City, cityWeatherForecast.City)
            .SetOnInsert(x => x.CountryCode, cityWeatherForecast.CountryCode)
            .SetOnInsert(x => x.SearchDate, cityWeatherForecast.SearchDate)
            .SetOnInsert(x => x.Temperature, cityWeatherForecast.Temperature)
            .SetOnInsert(x => x.Summary, cityWeatherForecast.Summary)
            .SetOnInsert(x => x.Icon, cityWeatherForecast.Icon);

        await cityWeatherForecastCollection.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = true },
            cancellationToken);

        // If you want the inserted doc back with Id, you can query by EventId.
        return cityWeatherForecast;
    }
}