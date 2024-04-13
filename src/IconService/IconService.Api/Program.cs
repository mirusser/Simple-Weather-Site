using Common.Presentation;
using IconService;
using IconService.Application;
using IconService.Infrastructure;
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
		.AddApplication()
		.AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
	if (builder.Environment.IsDevelopment())
	{
		app.UseDeveloperExceptionPage();
	}

	app.UseDefaultExceptionHandler();

	app.UseSwagger();
	app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IconService v1"));

	//app.UseHttpsRedirection();

	app.UseRouting();

	app.UseAuthorization();

	app.UseEndpoints(endpoints =>
	{
		endpoints.MapControllers();
	});
}

await app.RunWithLoggerAsync();