using Microsoft.Extensions.DependencyInjection;

namespace IconService.Api;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPresentation()
        {
            services.AddControllers();
        
            return services;
        }
    }
}