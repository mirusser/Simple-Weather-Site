using Common.Application.HealthChecks;
using Common.Mediator.DependencyInjection;
using Common.Presentation;
using Common.Shared;
using Hangfire;
using HangfireService.Clients;
using HangfireService.Clients.Contracts;
using HangfireService.Features.Filters;
using HangfireService.Settings;
using Polly;

var builder = WebApplication.CreateBuilder(args);
{
    builder.AddCommonPresentationLayer();

    builder.Services
        .AddMediator(AppDomain.CurrentDomain.GetAssemblies())
        .AddCustomMassTransit(builder.Configuration)
        .AddHangfireServices(builder.Configuration)
        .AddCommonHealthChecks();

    builder.Services.AddControllers();

    // TODO: use pipeline resilience from commons
    builder.Services.AddHttpClient<IHangfireHttpClient, HangfireHttpClient>()
        .AddTransientHttpErrorPolicy(builder =>
            builder.WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
        .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10)));
}

var app = builder.Build();
{
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseDefaultScalar();

    app
        .UseDefaultExceptionHandler()
        .UseHttpsRedirection()
        .UseRouting()
        .UseCommonHealthChecks()
        .UseAuthorization();

    app.MapControllers();

    app.UseHangfireDashboard("/dashboard", new DashboardOptions()
    {
        Authorization = [new AuthorizationFilter()]
    });

    app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();