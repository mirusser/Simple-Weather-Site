using Common.Mediator;
using IconService.Application.Icon.Commands.Seed;
using IconService.Domain.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IconService.Application.Icon.Services;

public sealed class MongoSeedHostedService(
    IServiceProvider sp,
    IOptions<MongoSettings> options,
    IWebHostEnvironment env,
    ILogger<MongoSeedHostedService> logger)
    : IHostedService
{
    private readonly MongoSettings.SeedingOptions options = options.Value.SeedingSettings;

    public async Task StartAsync(CancellationToken ct)
    {
        if (!options.Enabled)
        {
            logger.LogInformation("Mongo seeding disabled.");
            return;
        }

        if (options.OnlyInDevelopment && !env.IsDevelopment())
        {
            logger.LogInformation("Mongo seeding skipped (not Development).");
            return;
        }

        try
        {
            using var scope = sp.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var result = await mediator.SendAsync(new SeedCommand(), ct);

            logger.LogInformation(
                "Mongo seeded. Upserted={Upserted}, Matched={Matched}, Modified={Modified}",
                result.Upserted, result.Matched, result.Modified);
        }
        catch (OperationCanceledException)
        {
            // app is shutting down
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Mongo seeding failed.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}