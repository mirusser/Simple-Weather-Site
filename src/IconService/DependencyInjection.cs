using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace IconService;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "IconService", Version = "v1" });
        });

        return services;
    }
}