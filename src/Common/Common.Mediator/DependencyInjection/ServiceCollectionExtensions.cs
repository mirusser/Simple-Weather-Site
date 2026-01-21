using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Common.Mediator.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.TryAddScoped<IMediator, Mediator>();

        foreach (var asm in assemblies)
        {
            foreach (var type in asm.ExportedTypes.Where(t => t is { IsAbstract: false, IsInterface: false }))
            {
                // request handlers
                foreach (var it in type.GetInterfaces().Where(i =>
                             i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                {
                    services.TryAddTransient(it, type);
                }

                // notification handlers
                foreach (var it in type.GetInterfaces().Where(i =>
                             i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)))
                {
                    services.TryAddEnumerable(ServiceDescriptor.Transient(it, type));
                }

                // pipeline behaviors (usually registered explicitly, but scanning is fine too)
                foreach (var it in type.GetInterfaces().Where(i =>
                             i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)))
                {
                    //services.TryAddEnumerable(ServiceDescriptor.Transient(it, type));
                    
                    services.TryAddEnumerable(
                        ServiceDescriptor.Transient(it.GetGenericTypeDefinition(), type));
                }
            }
        }

        return services;
    }
}