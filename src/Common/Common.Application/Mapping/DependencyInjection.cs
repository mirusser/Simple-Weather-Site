﻿using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.Mapping;

public static class DependencyInjection
{
    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        var foo = Assembly.GetExecutingAssembly();
        var config = TypeAdapterConfig.GlobalSettings;
        var registers = config.Scan(foo);

        config.Apply(registers);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}