using System.Reflection;
using Common.Application.Behaviors;
using Common.Application.HealthChecks;
using Common.Application.Mapping;
using EmailService.Domain.Settings;
using EmailService.Listeners;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplication(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var executingAssembly = Assembly.GetExecutingAssembly();
		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(executingAssembly));

		services.AddMappings(executingAssembly);

		services.AddMassTransit(config =>
		{
			RabbitMQSettings rabbitMQSettings = new();
			configuration.GetSection(nameof(RabbitMQSettings)).Bind(rabbitMQSettings);

			config.AddConsumer<SendEmailListener>();
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
			options.WaitUntilStarted = true;

			// if specified, limits the wait time when starting the bus
			//options.StartTimeout = TimeSpan.FromSeconds(10);

			// if specified, limits the wait time when stopping the bus
			//options.StopTimeout = TimeSpan.FromSeconds(30);
		});

		services.AddScoped(
			typeof(IPipelineBehavior<,>),
			typeof(ValidationBehavior<,>));

		services.AddValidatorsFromAssembly(executingAssembly);

		services.AddTransient(
			typeof(IPipelineBehavior<,>),
			typeof(LoggingBehavior<,>));

		services.AddCommonHealthChecks(configuration);

		return services;
	}
}