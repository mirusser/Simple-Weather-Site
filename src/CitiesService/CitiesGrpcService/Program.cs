using CitiesGrpcService.Mappings;
using CitiesGrpcService.Services;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddGrpc();

    //Configure to user cors, needs: Grpc.AspNetCore.Web package
    //services.AddCors(o => o.AddPolicy("AllowAll", builder =>
    //{
    //    builder.AllowAnyOrigin()
    //           .AllowAnyMethod()
    //           .AllowAnyHeader()
    //           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    //}));

    builder.Services.AddApplicationLayer(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddMappings();
}

var app = builder.Build();
{
    if (builder.Environment.IsDevelopment())
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
        endpoints.MapGrpcService<CitiesGrpcService.Services.CitiesService>();

        endpoints.MapGet("/", async context =>
        {
            await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        });
    });

    app.Run();
}