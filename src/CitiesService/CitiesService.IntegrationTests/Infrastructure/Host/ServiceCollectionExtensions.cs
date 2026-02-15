using Microsoft.Extensions.DependencyInjection;

namespace CitiesService.IntegrationTests.Infrastructure.Host;

public static class ServiceCollectionExtensions
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
