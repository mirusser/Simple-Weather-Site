using IconService.Application.Common.Interfaces.Persistence;
using IconService.Domain.Settings;
using IconService.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IconService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.Configure<MongoSettings>(configuration.GetSection(nameof(MongoSettings)));
        services.Configure<ServiceSettings>(configuration.GetSection(nameof(ServiceSettings)));

        services.AddTransient(typeof(IMongoRepository<>), typeof(MongoRepository<>));

        services.AddSingleton(typeof(IMongoCollectionFactory<>), typeof(MongoCollectionFactory<>));

        return services;
    }
}