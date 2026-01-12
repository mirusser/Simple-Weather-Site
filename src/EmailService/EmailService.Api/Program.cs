using Common.Application.HealthChecks;
using Common.Presentation;
using Common.Shared;
using EmailService.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
{
    builder.AddCommonPresentationLayer();
    builder.Services.AddApplicationLayer(builder.Configuration);
}

var app = builder.Build();
{
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseDefaultExceptionHandler();
    app.UseDefaultScalar();
    app.UseHttpsRedirection();

    app
        .UseRouting()
        .UseCommonHealthChecks();

    app.UseAuthorization();

    app.MapControllers();
    app.MapGet("/ping", () => "pong");
    app.MapGet("/", () => $"EmailService in {builder.Environment.EnvironmentName} mode");
}

await app.RunWithLoggerAsync();