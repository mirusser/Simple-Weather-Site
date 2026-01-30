using System.Reflection;
using System.Text.Json.Serialization;
using Common.Application.HealthChecks;
using Common.Application.Mapping;
using Common.Infrastructure.Managers;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Mediator.DependencyInjection;
using Common.Presentation;
using Common.Shared;
using Hangfire;
using HangfireService.Features.Filters;
using HangfireService.Settings;
using Polly;
using Polly.Retry;

var builder = WebApplication.CreateBuilder(args);
{
    builder.AddCommonPresentationLayer();

    builder.Services
        .AddMediator(AppDomain.CurrentDomain.GetAssemblies())
        .AddCustomMassTransit(builder.Configuration)
        .AddHangfireServices(builder.Configuration)
        .AddCommonHealthChecks(builder.Configuration);

    builder.Services.AddMappings(Assembly.GetExecutingAssembly());
    
    builder.Services.AddControllers()
        .AddJsonOptions(o =>
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddHttpClient();

    var defaultPipe = new ResiliencePipelineOptions();
    builder.Configuration.GetSection($"{nameof(ResiliencePipelines)}:{nameof(ResiliencePipelines.Default)}")
        .Bind(defaultPipe);

    builder.Services.AddResiliencePipeline(defaultPipe.Name, pipeline =>
    {
        pipeline.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = defaultPipe.MaxRetryAttempts,
            Delay = TimeSpan.FromSeconds(defaultPipe.DelaySeconds),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = defaultPipe.UseJitter,
        });

        pipeline.AddTimeout(TimeSpan.FromSeconds(defaultPipe.TimeoutSeconds));
    });

    var healthPipe = new ResiliencePipelineOptions();
    builder.Configuration.GetSection($"{nameof(ResiliencePipelines)}:{nameof(ResiliencePipelines.Health)}")
        .Bind(healthPipe);

    builder.Services.AddResiliencePipeline(healthPipe.Name, pipeline =>
    {
        pipeline.AddTimeout(TimeSpan.FromSeconds(healthPipe.TimeoutSeconds));
    });

    builder.Services.AddSingleton<IHttpExecutor, HttpExecutor>();
    builder.Services.AddSingleton<IHttpRequestFactory, HttpRequestFactory>();
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
        .UseAuthorization();
    
    app.MapControllers();

    app.UseHangfireDashboard("/dashboard", new DashboardOptions()
    {
        Authorization = [new AuthorizationFilter()]
    });

    app.MapCommonHealthChecks();
    app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();
