using IconService.Application.Common.Interfaces.Persistence;
using IconService.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IconService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddTransient(typeof(IMongoRepository<>), typeof(MongoRepository<>));

        services.AddSingleton(typeof(IMongoCollectionFactory<>), typeof(MongoCollectionFactory<>));

        return services;
    }
}