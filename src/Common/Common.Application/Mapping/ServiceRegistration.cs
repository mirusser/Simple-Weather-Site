using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.Mapping;

public static class ServiceRegistration
{
    public static IServiceCollection AddMappings(this IServiceCollection services, Assembly? executingAssembly = null)
    {
        executingAssembly ??= Assembly.GetExecutingAssembly();
        var config = TypeAdapterConfig.GlobalSettings;
        var registers = config.Scan(executingAssembly);

        config.Apply(registers);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}