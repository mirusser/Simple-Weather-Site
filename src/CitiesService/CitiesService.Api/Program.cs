using CitiesService.Api;
using CitiesService.Application;
using CitiesService.Application.Telemetry;
using CitiesService.GraphQL;
using CitiesService.Infrastructure;
using Common.Application.HealthChecks;
using Common.Presentation;
using Common.Shared;
using Common.Telemetry;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
{
    builder.WebHost
        .CustomKestrelConfiguration(builder.Environment);
    
    builder.Services
        .AddInfrastructureLayer(builder.Configuration)
        .AddApplicationLayer(builder.Configuration)
        .AddPresentationLayer(builder.Configuration, builder.Environment)
        .AddGraphQlLayer(builder.Configuration);
    
    builder
        .AddCommonPresentationLayer(new CommonTelemetryOptions
        {
            MeterNames = [CitiesTelemetryConventions.Meters.Application],
            ActivitySourceNames =
            [
                CitiesTelemetryConventions.ActivitySources.Application,
                CitiesTelemetryConventions.ActivitySources.GraphQl
            ]
        });
}

var app = builder.Build();
{
    app.UseDefaultScalar();

    app
        .UseStaticFiles() // TODO: remove after this issue is fixed: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2130 (also wwwroot directory)
        .UseDefaultExceptionHandler()
        //.UseHttpsRedirection() // TODO: Redirect deletes authorization header - figure out/apply the solution https://stackoverflow.com/questions/28564961/authorization-header-is-lost-on-redirect
        .UseRouting()
        .UseCommonPrometheusMetrics(builder.Configuration)
        // .UseAuthentication()
        // .UseAuthorization()
        .UseCors("AllowAll");

    app.MapControllers();
    app.MapGraphQL();
    
    app.MapCommonHealthChecks();
    app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();
