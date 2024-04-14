using System.Reflection;
using Common.Application.HealthChecks;
using Common.Presentation;
using Common.Shared;
using Hangfire;
using HangfireService.Clients;
using HangfireService.Clients.Contracts;
using HangfireService.Features.Filters;
using HangfireService.Settings;
using Polly;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
{
	var executingAssembly = Assembly.GetExecutingAssembly();

	builder.Host.UseSerilog();

	builder.Services
		.AddCommonPresentationLayer(builder.Configuration)
		.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(executingAssembly))
		.AddCustomMassTransit(builder.Configuration)
		.AddHangfireServices(builder.Configuration)
		.AddCommonHealthChecks(builder.Configuration);

	builder.Services.AddControllers();

	builder.Services.AddHttpClient<IHangfireHttpClient, HangfireHttpClient>()
		.AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
		.AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10)));
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