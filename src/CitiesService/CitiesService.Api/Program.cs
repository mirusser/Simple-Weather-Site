using System;
using System.Linq;
using CitiesService.Api;
using CitiesService.Application;
using CitiesService.Infrastructure;
using CitiesService.Infrastructure.Contexts;
using Common.Application.HealthChecks;
using Common.Presentation;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
{
    builder.WebHost
        .CustomKestrelConfiguration(builder.Environment);

    builder
        .AddCommonPresentationLayer();

    builder.Services
        .AddInfrastructure(builder.Configuration)
        .AddApplicationLayer(builder.Configuration)
        .AddPresentationLayer(builder.Configuration, builder.Environment);
}

var app = builder.Build();
{
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();

        var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
        if (pending.Count > 0)
        {
            Console.WriteLine($"Pending migrations: {string.Concat(pending, ", ")}");
            Console.WriteLine("About to run migrations...");
            await db.Database.MigrateAsync();
            Console.WriteLine("Migrations applied.");
        }
    }

    app.UseDefaultScalar();

    app
        .UseStaticFiles() // TODO: remove after this issue is fixed: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2130 (also wwwroot directory)
        .UseDefaultExceptionHandler()
        //.UseHttpsRedirection() // TODO: Redirect deletes authorization header - figure out/apply the solution https://stackoverflow.com/questions/28564961/authorization-header-is-lost-on-redirect
        .UseRouting()
        .UseAuthentication()
        .UseAuthorization()
        .UseCors("AllowAll");

    app.MapControllers();
    app.MapCommonHealthChecks();
    app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();