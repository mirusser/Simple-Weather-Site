using CitiesService.Api;
using CitiesService.Application;
using CitiesService.Infrastructure;
using Common.Presentation;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
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
	if (builder.Environment.IsDevelopment())
	{
		app.UseDeveloperExceptionPage();
	}

	app
	.UseDefaultExceptionHandler()
	.UseDefaultSwagger()
	.UseApplicationLayer()
	.UseHttpsRedirection()
	.UseRouting()
	.UseAuthorization()
	.UseCors("AllowAll");

	app.MapControllers();
	app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();