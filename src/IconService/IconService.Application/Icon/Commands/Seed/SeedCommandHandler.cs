using Common.Mediator;
using IconService.Domain.Entities.Documents;
using IconService.Domain.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace IconService.Application.Icon.Commands.Seed;

public record SeedCommand : IRequest<SeedResult>;

public record SeedResult(int Upserted, int Matched, int Modified);

public class SeedCommandHandler(
    IMongoClient client,
    IOptions<MongoSettings> options,
    IWebHostEnvironment env)
    : IRequestHandler<SeedCommand, SeedResult>
{
    public async Task<SeedResult> Handle(
        SeedCommand request, 
        CancellationToken ct)
    {
        var settings = options.Value;

        var db = client.GetDatabase(settings.Database);
        var collection = db.GetCollection<IconDocument>(settings.IconsCollectionName);

        // locate file (put it in your project and copy to output)
        var path = Path.Combine(env.ContentRootPath, "Icons", "Icons-mongo-collection.json");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Seed file not found: {path}");
        }

        var json = await File.ReadAllTextAsync(path, ct);

        // Your JSON starts with [ ... ] so deserialize as a BsonArray first
        var array = BsonSerializer.Deserialize<BsonArray>(json);

        var models = new List<WriteModel<IconDocument>>(array.Count);

        foreach (var item in array)
        {
            var doc = BsonSerializer.Deserialize<IconDocument>(item.AsBsonDocument);

            // upsert by _id so you can run this endpoint multiple times
            var filter = Builders<IconDocument>.Filter.Eq(x => x.Id, doc.Id);
            var replace = new ReplaceOneModel<IconDocument>(filter, doc) { IsUpsert = true };
            models.Add(replace);
        }

        if (models.Count == 0)
        {
            return new SeedResult(0, 0, 0);
        }

        var result = await collection.BulkWriteAsync(
            models,
            new BulkWriteOptions { IsOrdered = false },
            ct);

        return new SeedResult(
            Upserted: (int)result.Upserts.Count,
            Matched: (int)result.MatchedCount,
            Modified: (int)result.ModifiedCount);
    }
}
