using Common.Application.HealthChecks;
using Common.Presentation;
using Common.Shared;
using IconService;
using IconService.Application;
using IconService.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
{
    builder.AddCommonPresentationLayer();

    builder.Services
        .AddPresentation()
        .AddApplication(builder.Configuration)
        .AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseDefaultExceptionHandler();
    app.UseDefaultScalar();

    //app.UseHttpsRedirection();

    app
        .UseRouting()
        .UseCommonHealthChecks();

    app.UseAuthorization();

    app.MapControllers();
}

await app.RunWithLoggerAsync();