using System.Reflection;
using HangfireService.Clients.Contracts;
using HangfireService.Clients;
using HangfireService.Settings;
using Serilog;
using Polly;
using Hangfire;
using Common.Presentation;
using Common.Shared;
using HangfireService.Features.Filters;
using Common.Application.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
{
	var executingAssembly = Assembly.GetExecutingAssembly();

	builder.Host.UseSerilog();

	builder.Services
		.AddCommonPresentationLayer(builder.Configuration)
		.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(executingAssembly))
		.AddCustomMassTransit(builder.Configuration)
		.AddHangfireServices(builder.Configuration);

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
	.UseAuthorization()
	.UseCommonHealthChecks();

	app.MapControllers();

	app.UseHangfireDashboard("/dashboard", new DashboardOptions()
	{
		Authorization = [new AuthorizationFilter()]
	});

	app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();