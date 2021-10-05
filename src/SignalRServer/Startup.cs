using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalRServer.Hubs.Site;
using SignalRServer.Listeners;
using SignalRServer.Services;
using SignalRServer.Services.Contracts;
using SignalRServer.Settings;

namespace SignalRServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MongoSettings>(Configuration.GetSection(nameof(MongoSettings)));
            services.Configure<HubMethods>(Configuration.GetSection(nameof(HubMethods)));

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed((_) => true)
                        .AllowCredentials();
                    });
            });

            services.AddSignalR();

            services.AddMassTransit(config =>
            {
                RabbitMQSettings rabbitMQSettings = new();
                Configuration.GetSection(nameof(RabbitMQSettings)).Bind(rabbitMQSettings);

                config.AddConsumer<CreatedCityWeatherForecastSearchListener>();
                config.SetKebabCaseEndpointNameFormatter();

                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(rabbitMQSettings.Host);
                    cfg.ConfigureEndpoints(ctx);
                });
            });
            services.AddMassTransitHostedService(waitUntilStarted: true);

            services.AddSingleton(typeof(IMongoCollectionFactory<>), typeof(MongoCollectionFactory<>));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<WeatherHistoryHub>($"/{nameof(WeatherHistoryHub)}");
            });

            //app.UseSignalR(routes =>
            //{
            //    routes.MapHub<TestHub>("/test");
            //});
        }
    }
}