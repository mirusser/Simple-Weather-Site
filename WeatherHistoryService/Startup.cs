using Convey;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Persistence.MongoDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherHistoryService.Mappings;
using WeatherHistoryService.Messages.Events.External;
using WeatherHistoryService.Mongo;
using WeatherHistoryService.Mongo.Documents;
using WeatherHistoryService.Services;
using WeatherHistoryService.Services.Contracts;

namespace WeatherHistoryService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MongoSettings>(Configuration.GetSection("mongo"));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherHistoryService", Version = "v1" });
            });

            var mongoSettings = new MongoSettings();
            Configuration.GetSection("mongo").Bind(mongoSettings);

            services.AddConvey()
                //    .AddConsul()
                .AddCommandHandlers()
                .AddEventHandlers()
                .AddQueryHandlers()
                // .AddServiceBusEventDispatcher()
                // .AddServiceBusCommandDispatcher()
                .AddInMemoryCommandDispatcher()
                .AddInMemoryEventDispatcher()
                .AddInMemoryQueryDispatcher()
                //    .AddRedis()
                .AddRabbitMq()
                .AddMongo()
                .AddMongoRepository<CityWeatherForecastDocument, Guid>(mongoSettings.CityWeatherForecastsCollectionName)
                .Build();

            //register autoMapper
            services.AddAutoMapper(typeof(Maps));
            
            //register services
            services.AddScoped<ICityWeatherForecastService, CityWeatherForecastService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseConvey();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherHistoryService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseRabbitMq()
                .SubscribeEvent<GotWeatherForecastEvent>();
        }
    }
}
