using CitiesService.Api;
using CitiesService.Application;
using CitiesService.Infrastructure;
using Common.Presentation;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
{
	builder.WebHost
		.CustomKestrelConfiguration(builder.Environment);

	builder.Host
		.UseSerilog();

	builder.Services
		.AddInfrastructure(builder.Configuration)
		.AddApplicationLayer(builder.Configuration)
		.AddCommonPresentationLayer(builder.Configuration)
		.AddPresentation(builder.Configuration, builder.Environment);
}

var app = builder.Build();
{
	app
	.UseStaticFiles() // TODO: remove after this issue is fixed: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2130 (also wwwroot directory)
	.UseDefaultExceptionHandler()
	.UseDefaultSwagger()
	//.UseHttpsRedirection() // TODO: Redirect deletes authorization header - figure out/apply the solution https://stackoverflow.com/questions/28564961/authorization-header-is-lost-on-redirect
	.UseRouting()
	.UseAuthentication()
	.UseAuthorization()
	.UseCors("AllowAll")
	.UseApplicationLayer(builder.Configuration);

	app.MapControllers();
	app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();