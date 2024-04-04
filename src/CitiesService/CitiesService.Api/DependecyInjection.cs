using CitiesService.Application.Features.HealthChecks;
using CitiesService.Infrastructure.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace CitiesService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "CitiesService", Version = "v1" });
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
        });

        services.AddHttpClient();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>()
            .AddCheck<CustomHealthCheck>(name: "Custom health check");

        return services;
    }
}