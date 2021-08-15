using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CitiesGrpcService;
using System.Net.Http;

namespace GrpcCitiesClient
{
    public static class ServiceExtensions
    {
        public static void AddGrpcCitiesClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGrpcClient<Cities.CitiesClient>("Cities", o =>
            {
                o.Address = new Uri("https://localhost:5031");
            })
            //.EnableCallContextPropagation() 
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                return handler;
            });

            services.AddScoped<ICitiesClient, CitiesClient>();
        }
    }
}
