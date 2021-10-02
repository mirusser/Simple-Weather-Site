using System.Linq;
using System.Text.Json;
using Application;
using Application.HealthChecks;
using CitiesService.Exceptions.Handlers;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace CitiesService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationLayer(Configuration);
            services.AddApplicationLayerAutomapper();
            services.AddPersistenceInfrastructure(Configuration);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CitiesService", Version = "v1" });
            });

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

            services.AddHttpClient();

            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>()
                .AddCheck<CustomHealthCheck>(name: "Custom Healthcheck");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseServiceExceptionHandler();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CitiesService v1"));

            app.UseApplicationLayer();

            app.UseHttpsRedirection();
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

            #endregion Healthchecks

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapHealthChecks("/health");
                endpoints.MapGet("/ping", ctx => ctx.Response.WriteAsync("pong"));
            });
        }
    }
}