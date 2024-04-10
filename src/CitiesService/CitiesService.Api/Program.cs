using CitiesService.Api;
using CitiesService.Application;
using CitiesService.Infrastructure;
using Common.Presentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

	app.UseServiceExceptionHandler();
	app.UseSwagger();
	app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CitiesService v1"));

	app.UseApplicationLayer();

	app.UseHttpsRedirection();
	app.UseRouting();

	app.UseAuthorization();

	app.UseCors("AllowAll");

	app.MapControllers();
	app.MapHealthChecks("/health");
	app.MapGet("/ping", ctx => ctx.Response.WriteAsync("pong"));

	WebApplicationStartup.Run(app);
}