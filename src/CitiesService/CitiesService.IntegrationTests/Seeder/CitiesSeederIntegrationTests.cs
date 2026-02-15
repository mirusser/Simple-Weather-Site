using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Services;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Settings;
using CitiesService.Infrastructure.Repositories;
using CitiesService.IntegrationTests.Infrastructure.Collections;
using CitiesService.IntegrationTests.Infrastructure.Db;
using Common.Testing.Http;
using Common.Testing.IO;
using Common.Testing.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Polly.Registry;
using Xunit;

namespace CitiesService.IntegrationTests.Seeder;

/// <summary>
/// Exercises the real seeding flow (download -> decompress -> deserialize -> insert) against
/// a real SQL Server, but without external network calls (HTTP is faked in-process).
/// </summary>
[Collection(SqlServerCollection.Name)]
public class CitiesSeederIntegrationTests(SqlServerFixture sql)
{
    [SqlServerFact]
    public async Task SeedIfEmptyAsync_DownloadsDecompressesAndSavesCities()
    {
        using var tempDir = TempDirectory.Create("CitiesService.IntegrationTests");

        var compressedPath = tempDir.Combine("city.list.json.gz");
        var decompressedPath = tempDir.Combine("city.list.json");
        var url = "http://local.test/city.list.json.gz";

        var dbName = DbTestHelpers.CreateDatabaseName("cities_seed");
        var cs = sql.GetConnectionStringForDatabase(dbName);

        {
            await using (var db = DbTestHelpers.CreateDbContext(cs))
            {
                await db.Database.MigrateAsync();
            }

            var services = new ServiceCollection();
            services.AddLogging();

            services.AddSingleton(NullLogger<CitiesSeeder>.Instance);
            services.AddSingleton(JsonSerializerOptions.Web);

            // Polly v8 pipeline used by CitiesSeeder for HTTP calls (minimal provider for tests)
            services.AddSingleton<ResiliencePipelineProvider<string>, TestPipelineProvider>();

            var gzPayload = CreateGzPayload();
            var handler = new StubHttpMessageHandler((request, _) =>
            {
                if (request.RequestUri?.ToString() != url)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(gzPayload)
                });
            });

            services.AddSingleton<IHttpClientFactory>(new SingleClientFactory(new HttpClient(handler)));

            services.AddSingleton<IOptions<FileUrlsAndPaths>>(
                Options.Create(new FileUrlsAndPaths
                {
                    CityListFileUrl = url,
                    CompressedCityListFilePath = compressedPath,
                    DecompressedCityListFilePath = decompressedPath,
                }));

            services.AddScoped(_ => DbTestHelpers.CreateDbContext(cs));
            services.AddScoped<IGenericRepository<CityInfo>, GenericRepository<CityInfo>>();
            services.AddScoped<ICitiesSeeder, CitiesSeeder>();

            await using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<ICitiesSeeder>();

            var result = await seeder.SeedIfEmptyAsync(CancellationToken.None);
            Assert.True(result);

            var repo = scope.ServiceProvider.GetRequiredService<IGenericRepository<CityInfo>>();
            var count = await repo.FindAll(_ => true).CountAsync();
            Assert.Equal(2, count);
        }
    }

    private static byte[] CreateGzPayload()
    {
        // Must match GetCityResult JSON structure expected by CitiesSeeder.
        var json = "[" +
                   "{\"id\": 100, \"name\": \"Alpha\", \"state\": null, \"country\": \"AA\", \"coord\": {\"lon\": 1.1, \"lat\": 2.2}}," +
                   "{\"id\": 200, \"name\": \"Bravo\", \"state\": \"S\", \"country\": \"BB\", \"coord\": {\"lon\": 3.3, \"lat\": 4.4}}" +
                   "]";

        var bytes = Encoding.UTF8.GetBytes(json);
        using var ms = new MemoryStream();
        using (var gzip = new GZipStream(ms, CompressionMode.Compress, leaveOpen: true))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }

        return ms.ToArray();
    }

    private sealed class TestPipelineProvider : ResiliencePipelineProvider<string>
    {
        private readonly Polly.ResiliencePipeline pipeline = new Polly.ResiliencePipelineBuilder().Build();

        public override bool TryGetPipeline(string key, out Polly.ResiliencePipeline resiliencePipeline)
        {
            resiliencePipeline = pipeline;
            return true;
        }

        public override bool TryGetPipeline<TResult>(string key, out Polly.ResiliencePipeline<TResult> resiliencePipeline)
        {
            resiliencePipeline = new Polly.ResiliencePipelineBuilder<TResult>().Build();
            return true;
        }
    }
}
