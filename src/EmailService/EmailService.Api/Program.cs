using Common.Application.HealthChecks;
using Common.Presentation;
using Common.Shared;
using EmailService.Application;
using EmailService.Domain.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Host.UseSerilog();
    builder.Services.AddSharedLayer(builder.Configuration);
    builder.Services.AddCommonPresentationLayer(builder.Configuration);
    builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));

    builder.Services.AddApplication(builder.Configuration);

    builder.Services.AddControllers();
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

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapGet("/ping", ctx => ctx.Response.WriteAsync("pong"));
        endpoints.MapGet("",
            ctx => ctx.Response.WriteAsync($"EmailService in {builder.Environment.EnvironmentName} mode"));
    });
}

await app.RunWithLoggerAsync();