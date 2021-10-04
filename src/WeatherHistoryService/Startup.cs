using System.Reflection;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WeatherHistoryService.Exceptions.Handler;
using WeatherHistoryService.Listeners;
using WeatherHistoryService.Mappings;
using WeatherHistoryService.Mongo;
using WeatherHistoryService.Services;
using WeatherHistoryService.Services.Contracts;
using WeatherHistoryService.Settings;

namespace WeatherHistoryService
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

            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(typeof(Maps));

            services.AddMassTransit(config =>
            {
                RabbitMQSettings rabbitMQSettings = new();
                Configuration.GetSection(nameof(RabbitMQSettings)).Bind(rabbitMQSettings);

                config.AddConsumer<GotWeatherForecastListener>();
                config.SetKebabCaseEndpointNameFormatter();

                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(rabbitMQSettings.Host);
                    cfg.ConfigureEndpoints(ctx);
                });
            });
            services.AddMassTransitHostedService(waitUntilStarted: true);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherHistoryService", Version = "v1" });
            });

            services.AddHealthChecks();

            //register services
            services.AddSingleton(typeof(IMongoCollectionFactory<>), typeof(MongoCollectionFactory<>));
            services.AddScoped<ICityWeatherForecastService, CityWeatherForecastService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region Swagger

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherHistoryService v1"));

            #endregion Swagger

            app.UseServiceExceptionHandler();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
                endpoints.MapGet("/ping", ctx => ctx.Response.WriteAsync("pong"));
            });
        }
    }
}