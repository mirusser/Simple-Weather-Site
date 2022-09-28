using System.Linq;
using System.Text.Json;
using CitiesService;
using CitiesService.Application;
using CitiesService.Contracts.HealthCheck;
using CitiesService.Infrastructure;
using Common.Presentation;
using Common.Presentation.Exceptions.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Host.UseSerilog();

    builder.Services
        .AddPresentation()
        .AddApplicationLayer(builder.Configuration)
        .AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseServiceExceptionHandler();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CitiesService v1"));

    app.UseApplicationLayer();

    //app.UseHttpsRedirection();
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

    WebApplicationStartup.Run(app);
}