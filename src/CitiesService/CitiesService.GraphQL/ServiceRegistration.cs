using CitiesService.GraphQL.Types;
using CitiesService.Infrastructure.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CitiesService.GraphQL;

public static class ServiceRegistration
{
    public static IServiceCollection AddGraphQlLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddGraphQLServer()
            .RegisterDbContextFactory<ApplicationDbContext>()
            .AddDbContextCursorPagingProvider() // true EF cursor/seek paging provider
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .AddQueryType<CityQueries>()
            .AddMutationType<CityMutations>()
            .AddType<CityInfoType>()
            .AddType<PatchCityInputType>();
        
        services.AddSingleton<GraphQlExecutableHealthCheck>();

        return services;
    }
}