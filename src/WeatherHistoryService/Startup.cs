using Convey;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WeatherHistoryService.Exceptions.Handler;
using WeatherHistoryService.Mappings;
using WeatherHistoryService.Messages.Events.External;
using WeatherHistoryService.Mongo;
using WeatherHistoryService.Services;
using WeatherHistoryService.Services.Contracts;

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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherHistoryService", Version = "v1" });
            });

            var mongoSettings = new MongoSettings();
            Configuration.GetSection(nameof(MongoSettings)).Bind(mongoSettings);

            services.AddLogging();

            services.AddConvey()
                //    .AddConsul()
                .AddCommandHandlers()
                .AddEventHandlers()
                .AddQueryHandlers()
                .AddServiceBusEventDispatcher()
                .AddServiceBusCommandDispatcher()
                .AddInMemoryCommandDispatcher()
                .AddInMemoryEventDispatcher()
                .AddInMemoryQueryDispatcher()
                //    .AddRedis()
                .AddRabbitMq()
                //.AddMongo()
                .Build();

            //register heatlChecks
            services.AddHealthChecks();

            //register autoMapper
            services.AddAutoMapper(typeof(Maps));

            //register services
            services.AddSingleton(typeof(IMongoCollectionFactory<>), typeof(MongoCollectionFactory<>));
            services.AddScoped<ICityWeatherForecastService, CityWeatherForecastService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseConvey();

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

            app.UseRabbitMq()
                .SubscribeEvent<GotWeatherForecastEvent>();
        }
    }
}