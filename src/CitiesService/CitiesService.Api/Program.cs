using CitiesService.Api;
using CitiesService.Application;
using CitiesService.Infrastructure;
using Common.Presentation;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
{
	builder.Host.UseSerilog();

	builder.Services
		.AddCommonPresentationLayer(builder.Configuration)
		.AddPresentation()
		.AddApplicationLayer(builder.Configuration)
		.AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
	app
	.UseStaticFiles() // TODO: remove after this issue is fixed: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2130 (also wwwroot directory)
	.UseDefaultExceptionHandler()
	.UseDefaultSwagger()
	.UseHttpsRedirection()
	.UseRouting()
	.UseAuthorization()
	.UseCors("AllowAll")
	.UseApplicationLayer();

	app.MapControllers();
	app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();