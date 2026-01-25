using System.Reflection;
using CitiesGrpcService.Services;
using CitiesService.Application;
using CitiesService.Infrastructure;
using Common.Application.Mapping;
using Common.Presentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
{
    var executingAssembly = Assembly.GetExecutingAssembly();

    builder.AddCommonPresentationLayer();

    builder.Services.AddGrpc();

    //Configure to user cors, needs: Grpc.AspNetCore.Web package
    //services.AddCors(o => o.AddPolicy("AllowAll", builder =>
    //{
    //    builder.AllowAnyOrigin()
    //           .AllowAnyMethod()
    //           .AllowAnyHeader()
    //           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    //}));

    builder.Services
        .AddApplicationLayer(builder.Configuration)
        .AddInfrastructure(builder.Configuration);

    builder.Services.AddHttpClient();
    builder.Services.AddMappings(executingAssembly);
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

    //To use cors
    //app.MapGrpcService<GreeterService>().RequireCors("AllowAll");
    app.MapGrpcService<GreeterService>();
    app.MapGrpcService<CitiesGrpcService.Services.CitiesService>();
    app.MapGet("/",
        async context =>
        {
            await context.Response.WriteAsync(
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        });

    await app.RunWithLoggerAsync();
}