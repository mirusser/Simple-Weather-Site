using System.Reflection;
using CitiesService.Application.Features.Listeners;
using CitiesService.Domain.Settings;
using Common.Application.Behaviors;
using Common.Application.HealthChecks;
using Common.Application.Mapping;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CitiesService.Application;

public static class ServiceRegistration
{
	public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
	{
		var executingAssembly = Assembly.GetExecutingAssembly();

		services.AddValidatorsFromAssembly(executingAssembly);
		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(executingAssembly));

		services.AddMassTransit(config =>
		{
			RabbitMQSettings rabbitMQSettings = new();
			configuration
				.GetSection(nameof(RabbitMQSettings))
				.Bind(rabbitMQSettings);

			config.AddConsumer<JobListener>();

			config.SetKebabCaseEndpointNameFormatter();
			config.UsingRabbitMq((ctx, cfg) =>
			{
				cfg.Host(rabbitMQSettings.Host);
				cfg.ConfigureEndpoints(ctx);
			});
		});

		services.AddOptions<MassTransitHostOptions>()
			.Configure(options =>
			{
				// if specified, waits until the bus is started before
				// returning from IHostedService.StartAsync
				// default is false
				options.WaitUntilStarted = false;

				// if specified, limits the wait time when starting the bus
				//options.StartTimeout = TimeSpan.FromSeconds(10);

				// if specified, limits the wait time when stopping the bus
				//options.StopTimeout = TimeSpan.FromSeconds(30);
			});

		services.AddMappings(executingAssembly);

		return services;
	}

	public static IApplicationBuilder UseApplicationLayer(this IApplicationBuilder app, IConfiguration configuration)
	{
		app.UseCommonHealthChecks();

		return app;
	}
}