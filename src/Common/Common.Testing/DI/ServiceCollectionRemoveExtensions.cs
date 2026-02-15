using Microsoft.Extensions.DependencyInjection;

namespace Common.Testing.DI;

/// <summary>
/// Small helper extensions for integration tests that need to modify DI registrations.
///
/// Why: In integration tests we often want to disable hosted services (seeders, message bus)
/// and replace external dependencies (Redis, etc.) to keep tests fast and deterministic.
/// </summary>
public static class ServiceCollectionRemoveExtensions
{
    extension(IServiceCollection services)
    {
        public void RemoveHostedServiceByTypeName(string typeNameContains)
        {
            var descriptors = services
                .Where(d => d.ServiceType.FullName == "Microsoft.Extensions.Hosting.IHostedService")
                .ToList();

            foreach (var d in descriptors)
            {
                var impl = d.ImplementationType?.FullName ?? d.ImplementationInstance?.GetType().FullName;
                if (impl is not null && impl.Contains(typeNameContains, StringComparison.OrdinalIgnoreCase))
                {
                    services.Remove(d);
                }
            }
        }

        public void RemoveServiceByTypeFullName(string serviceTypeFullNameContains)
        {
            var descriptors = services
                .Where(d => d.ServiceType.FullName?.Contains(serviceTypeFullNameContains, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            foreach (var d in descriptors)
            {
                services.Remove(d);
            }
        }
    }
}
