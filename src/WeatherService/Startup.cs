using System;
using System.Reflection;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using WeatherService.Clients;
using WeatherService.HealthCheck;
using WeatherService.Middleware.Exceptions.Handlers;
using WeatherService.Settings;

namespace WeatherService
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
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddMassTransit(config =>
            {
                RabbitMQSettings rabbitMQSettings = new();
                Configuration.GetSection(nameof(RabbitMQSettings)).Bind(rabbitMQSettings);

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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherService", Version = "v1" });
            });

            services.AddHttpClient<WeatherClient>()
                 .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                 .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10)));

            services.AddHealthChecks()
                .AddCheck<ExternalEndpointHealthCheck>("OpenWeather");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseServiceExceptionHandler();

            #region Swagger

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherService v1"));

            #endregion Swagger

            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/api/weatherforecast/health");
            });
        }
    }
}