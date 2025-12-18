using System;
using System.Reflection;
using Common.Application.HealthChecks;
using Common.Application.Mapping;
using Common.Contracts.HealthCheck;
using Common.Mediator.DependencyInjection;
using Common.Presentation;
using Common.Shared;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Polly;
using Serilog;
using WeatherService.Clients;
using WeatherService.HealthCheck;
using WeatherService.Settings;

var builder = WebApplication.CreateBuilder(args);
{
	var executingAssembly = Assembly.GetExecutingAssembly();
	builder.Host.UseSerilog();

	builder.Services.AddCommonPresentationLayer(builder.Configuration);
	builder.Services.Configure<ServiceSettings>(builder.Configuration.GetSection(nameof(ServiceSettings)));

	builder.Services.AddMediator(AppDomain.CurrentDomain.GetAssemblies());

	builder.Services
		.AddMassTransit(config =>
		{
			RabbitMQSettings rabbitMQSettings = new();
			builder.Configuration.GetSection(nameof(RabbitMQSettings)).Bind(rabbitMQSettings);

			config.SetKebabCaseEndpointNameFormatter();

			config.UsingRabbitMq((ctx, cfg) =>
			{
				cfg.Host(rabbitMQSettings.Host);
				cfg.ConfigureEndpoints(ctx);
			});
		});

	builder.Services
		.AddOptions<MassTransitHostOptions>()
		.Configure(options =>
		{
			// if specified, waits until the bus is started before
			// returning from IHostedService.StartAsync
			// default is false
			options.WaitUntilStarted = true;

			// if specified, limits the wait time when starting the bus
			options.StartTimeout = TimeSpan.FromSeconds(10);

			// if specified, limits the wait time when stopping the bus
			options.StopTimeout = TimeSpan.FromSeconds(30);
		});

	builder.Services.AddMappings(executingAssembly);

	builder.Services.AddSharedLayer(builder.Configuration);
	builder.Services.AddControllers();

	// TODO: use resilience pipeline from commons
	builder.Services.AddHttpClient<WeatherClient>()
		 .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
		 .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10)));

	builder.Services.AddCommonHealthChecks(builder.Configuration)
		.AddCheck<OpenWeatherExternalEndpointHealthCheck>(
			name: "OpenWeather",
			failureStatus: HealthStatus.Degraded,
			tags: [nameof(HealthChecksTags.Database)]);
}

var app = builder.Build();
{
	if (builder.Environment.IsDevelopment())
	{
		app.UseDeveloperExceptionPage();
	}

	app.UseDefaultExceptionHandler();

	app.UseDefaultScalar();

	//app.UseHttpsRedirection();
	app
	.UseRouting()
	.UseCommonHealthChecks();
	app.UseAuthorization();

	app.MapControllers();
}

await app.RunWithLoggerAsync();