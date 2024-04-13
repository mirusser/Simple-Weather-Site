using System;
using System.Reflection;
using Common.Application.HealthChecks;
using Common.Application.Mapping;
using Common.Contracts.HealthCheck;
using Common.Presentation;
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

	builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(executingAssembly));

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

	builder.Services.AddControllers();
	builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "WeatherService", Version = "v1" }));

	builder.Services.AddHttpClient<WeatherClient>()
		 .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
		 .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10)));

	builder.Services.AddHealthChecks()
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

	#region Swagger

	app.UseSwagger();
	app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherService v1"));

	#endregion Swagger

	//app.UseHttpsRedirection();
	app.UseRouting();
	app.UseAuthorization();

	app.MapControllers();
	app.UseCommonHealthChecks();
}

await app.RunWithLoggerAsync();