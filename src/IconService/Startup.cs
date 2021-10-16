using System.Reflection;
using IconService.Exceptions;
using IconService.Mappings;
using IconService.Services;
using IconService.Settings;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace IconService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MongoSettings>(Configuration.GetSection(nameof(MongoSettings)));
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

            services.AddSingleton(typeof(IMongoCollectionFactory<>), typeof(MongoCollectionFactory<>));
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "IconService", Version = "v1" });
            });

            //register autoMapper
            services.AddAutoMapper(typeof(Maps));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware(typeof(ExceptionHandlerMiddleware));

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IconService v1"));

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}