﻿using Application;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CitiesGrpcService
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
            services.AddGrpc();

            //Configure to user cors, needs: Grpc.AspNetCore.Web package
            //services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            //{
            //    builder.AllowAnyOrigin()
            //           .AllowAnyMethod()
            //           .AllowAnyHeader()
            //           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
            //}));

            services.AddApplicationLayer(Configuration);
            services.AddPersistenceInfrastructure(Configuration);

            services.AddAutoMapper(typeof(Mappings.Maps));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            //Configure to user cors, needs: Grpc.AspNetCore.Web package
            //app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true }); // Must be added between UseRouting and UseEndpoints
            //app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                //To use cors
                //endpoints.MapGrpcService<GreeterService>().RequireCors("AllowAll");

                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<CitiesService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}