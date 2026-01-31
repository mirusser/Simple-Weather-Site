using System.Reflection;
using Common.Application.HealthChecks;
using Common.Mediator.DependencyInjection;
using Common.Presentation;
using Common.Shared;
using BackupService.Application;
using BackupService.Application.Services;
using Common.Application.Mapping;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddControllers();
    builder.Services.AddMediator(AppDomain.CurrentDomain.GetAssemblies());
    builder.Services.AddMappings(Assembly.GetExecutingAssembly());
    builder.Services.AddApplicationLayer(builder.Configuration);

    builder.AddCommonPresentationLayer();
    
    builder.Services.AddCommonHealthChecks(builder.Configuration);
}


var app = builder.Build();
{
    app.UseDefaultScalar();

    app
        .UseDefaultExceptionHandler()
        .UseHttpsRedirection()
        .UseRouting();
    // .UseAuthentication()
    // .UseAuthorization()

    app.MapControllers();
    app.MapCommonHealthChecks();
    app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();
