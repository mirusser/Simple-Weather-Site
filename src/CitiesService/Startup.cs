using CitiesService.Data;
using CitiesService.Data.DatabaseModels;
using CitiesService.Exceptions.Handlers;
using CitiesService.Logic.Managers;
using CitiesService.Logic.Managers.Contracts;
using CitiesService.Logic.Repositories;
using CitiesService.Logic.Repositories.Contracts;
using CitiesService.Logic.Mappings;
using CitiesService.Settings;
using Convey;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using CitiesService.Data.Models;
using System.Text.Json;
using CitiesService.Logic.HealthChecks;

namespace CitiesService
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
            services.Configure<FileUrlsAndPaths>(Configuration.GetSection(nameof(FileUrlsAndPaths)));
            services.Configure<ConnectionStrings>(Configuration.GetSection(nameof(ConnectionStrings)));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddTransient<ApplicationDbContext>();

            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
            });

            services.AddConvey()
                //    .AddConsul()
                .AddCommandHandlers()
                .AddEventHandlers()
                .AddQueryHandlers()
                .AddServiceBusEventDispatcher()
                .AddServiceBusCommandDispatcher()
                .AddInMemoryCommandDispatcher()
                // .AddInMemoryEventDispatcher()
                .AddInMemoryQueryDispatcher()
                //    .AddRedis()
                .AddRabbitMq()
                //.AddMongo()
                .Build();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CitiesService", Version = "v1" });
            });

            services.AddHttpClient();

            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>()
                .AddCheck<CustomHealthCheck>(name: "Custom Healthcheck"); ;

            services.AddAutoMapper(typeof(Maps));

            #region Caching
            services.AddMemoryCache();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost:6379";
            });
            #endregion

            services.AddTransient<IGenericRepository<CityInfo>, GenericRepository<CityInfo>>();

            services.AddTransient<ICityManager, CityManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CitiesService v1"));
            #endregion

            app.UseServiceExceptionHandler();
            app.UseHttpsRedirection();
            app.UseConvey();
            app.UseRouting();

            app.UseAuthorization();

            app.UseCors("AllowAll");

            #region Healthchecks
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new HealthCheckReponse
                    {
                        Status = report.Status.ToString(),
                        HealthCheckDuration = report.TotalDuration,
                        HealthChecks = report.Entries.Select(x => new IndividualHealthCheckResponse
                        {
                            Component = x.Key,
                            Status = x.Value.Status.ToString(),
                            Description = x.Value.Description
                        })
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                }
            });
            #endregion

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapHealthChecks("/health");
                endpoints.MapGet("/ping", ctx => ctx.Response.WriteAsync("pong"));
            });


        }
    }
}
