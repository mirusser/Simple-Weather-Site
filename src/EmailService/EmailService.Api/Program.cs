using Common.Application.HealthChecks;
using Common.Presentation;
using Common.Shared;
using EmailService.Application;
using EmailService.Domain.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Host.UseSerilog();
    builder.Services.AddSharedLayer(builder.Configuration);
    builder.Services.AddCommonPresentationLayer(builder.Configuration);
    
    builder.Services.AddOptions<MailSettings>()
        .Bind(builder.Configuration.GetSection(nameof(MailSettings)))
        .Validate(s => !string.IsNullOrWhiteSpace(s.From), "MailSettings.From is required.")
        .Validate(s => !string.IsNullOrWhiteSpace(s.DefaultEmailReceiver), "MailSettings.DefaultEmailReceiver is required.")
        .ValidateOnStart();
    
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

    app.MapControllers();
    app.MapGet("/ping", () => "pong");
    app.MapGet("/", () => $"EmailService in {builder.Environment.EnvironmentName} mode");
}

await app.RunWithLoggerAsync();