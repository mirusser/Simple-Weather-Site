using System.Reflection;
using HangfireService.Clients.Contracts;
using HangfireService.Clients;
using HangfireService.Settings;
using Serilog;
using Polly;
using Hangfire;
using Common.Presentation;
using HangfireService.Features.Filters;

var builder = WebApplication.CreateBuilder(args);
{
	var executingAssembly = Assembly.GetExecutingAssembly();

	builder.Host.UseSerilog();

	builder.Services
		.AddCommonPresentationLayer(builder.Configuration);

	builder.Services.AddControllers();
	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddSwaggerGen();

	builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(executingAssembly));
	builder.Services.AddRabbitMQ(builder.Configuration);
	builder.Services.AddHangfireServices(builder.Configuration);
	builder.Services.AddHttpClient<ICallEndpointClient, CallEndpointClient>()
		.AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
		.AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10)));
}

var app = builder.Build();
{
	if (builder.Environment.IsDevelopment())
	{
		app.UseDeveloperExceptionPage();
	}

	app.UseSwagger();
	app.UseSwaggerUI();

	app.UseHttpsRedirection();
	app.UseRouting();

	//app.UseAuthorization();

	app.MapControllers();

	app.UseHangfireDashboard("/dashboard", new DashboardOptions()
	{
		Authorization = [new AuthorizationFilter()]
	});

	app.MapHealthChecks("/health");
}

WebApplicationStartup.Run(app);