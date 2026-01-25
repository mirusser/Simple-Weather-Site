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

    if (!builder.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app
        .UseRouting()
        .UseAuthorization();

    app.MapControllers();
    app.MapCommonHealthChecks();
    app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();