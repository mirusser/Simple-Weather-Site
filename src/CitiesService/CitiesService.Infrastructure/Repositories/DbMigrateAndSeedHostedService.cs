using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Features.City.Services;
using CitiesService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CitiesService.Infrastructure.Repositories;

public sealed class DbMigrateAndSeedHostedService(
    IServiceProvider sp,
    ILogger<DbMigrateAndSeedHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<ICitiesSeeder>();

        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation("Applying migrations: {Migrations}", string.Join(", ", pending));
            await db.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Migrations applied.");
        }

        await seeder.SeedIfEmptyAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
