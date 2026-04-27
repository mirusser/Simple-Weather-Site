using System.Reflection;
using CitiesGrpcService.Telemetry;
using CitiesGrpcService.Services;
using CitiesService.Application;
using CitiesService.Application.Telemetry;
using CitiesService.Infrastructure;
using CitiesService.Infrastructure.HealthChecks;
using Common.Application.HealthChecks;
using Common.Application.Mapping;
using Common.Contracts.HealthCheck;
using Common.Presentation;
using Common.Shared;
using Common.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
{
    if (!builder.Environment.IsDevelopment())
    {
        // TODO: get ports from config (appsettings maybe)
        builder.WebHost.ConfigureKestrel(options =>
        {
            // HTTP/1 for health & diagnostics
            options.ListenAnyIP(80, lo => lo.Protocols = HttpProtocols.Http1);

            // gRPC (HTTP/2, no TLS inside container)
            options.ListenAnyIP(5043, lo => lo.Protocols = HttpProtocols.Http2);
        });
    }

    builder.AddCommonPresentationLayer(new CommonTelemetryOptions
    {
        MeterNames =
        [
            CitiesTelemetry.ApplicationMeterName,
            CitiesGrpcTelemetry.MeterName
        ],
        ActivitySourceNames =
        [
            CitiesTelemetry.ApplicationActivitySourceName,
            CitiesGrpcTelemetry.ActivitySourceName
        ]
    });

    builder.Services.AddGrpc();

    //Configure to user cors, needs: Grpc.AspNetCore.Web package
    //services.AddCors(o => o.AddPolicy("AllowAll", builder =>
    //{
    //    builder.AllowAnyOrigin()
    //           .AllowAnyMethod()
    //           .AllowAnyHeader()
    //           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    //}));

    builder.Services
        .AddApplicationLayer(builder.Configuration)
        .AddInfrastructureLayer(builder.Configuration);

    builder.Services.AddHttpClient();

    var executingAssembly = Assembly.GetExecutingAssembly();
    builder.Services.AddMappings(executingAssembly);

    builder.Services
        .AddCommonHealthChecks(builder.Configuration)
        .AddCitiesServiceDbContextCheck(
            name: "DB health check",
            failureStatus: HealthStatus.Unhealthy,
            tags: [HealthChecksTags.Ready, HealthChecksTags.Database]);
}

var app = builder.Build();
{
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseRouting();
    app.UseCommonPrometheusMetrics(builder.Configuration);

    //Configure to user cors, needs: Grpc.AspNetCore.Web package
    //app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true }); // Must be added between UseRouting and UseEndpoints
    //app.UseCors();

    //To use cors
    //app.MapGrpcService<GreeterService>().RequireCors("AllowAll");
    app.MapGrpcService<GreeterService>();
    app.MapGrpcService<CitiesGrpcService.Services.CitiesService>();


    app.MapCommonHealthChecks();
    app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();
