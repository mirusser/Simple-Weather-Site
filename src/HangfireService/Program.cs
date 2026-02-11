using System.Reflection;
using System.Text.Json.Serialization;
using Common.Application.HealthChecks;
using Common.Application.Mapping;
using Common.Contracts.HealthCheck;
using Common.Infrastructure;
using Common.Mediator.DependencyInjection;
using Common.Presentation;
using Common.Shared;
using Hangfire;
using HangfireService.Features.Filters;
using HangfireService.Features.HealthChecks;
using HangfireService.Features.Jobs;
using HangfireService.Settings;

var builder = WebApplication.CreateBuilder(args);
{
    builder.AddCommonPresentationLayer();

    builder.Services
        .AddMediator(AppDomain.CurrentDomain.GetAssemblies())
        .AddCustomMassTransit(builder.Configuration)
        .AddHangfireServices(builder.Configuration);

    builder.Services.AddMappings(Assembly.GetExecutingAssembly());
    
    builder.Services.AddControllers()
        .AddJsonOptions(o =>
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddHttpClient();

    builder.Services.AddCommonResiliencePipelines(builder.Configuration);
    builder.Services.AddHttpExecutor();
    builder.Services.AddSingleton<IHangfireJobContextAccessor, HangfireJobContextAccessor>();

    builder.Services.Configure<BackupJobSettings>(
        builder.Configuration.GetSection(nameof(BackupJobSettings)));

    builder.Services
        .AddCommonHealthChecks(builder.Configuration)
        .AddCheck<BackupJobRegisteredHealthCheck>(
            "Backup job registered",
            tags: [HealthChecksTags.Ready]);
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
        //.UseHttpsRedirection()
        .UseRouting();
        //.UseAuthorization();
    
    app.MapControllers();
    app.RegisterRecurringBackupJobs();

    app.UseHangfireDashboard("/dashboard", new DashboardOptions()
    {
        Authorization = [new AuthorizationFilter()]
    });

    app.MapCommonHealthChecks();
    app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();
