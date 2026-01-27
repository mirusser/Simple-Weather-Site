using System.Reflection;
using Common.Application.HealthChecks;
using Common.Application.Mapping;
using Common.Mediator.DependencyInjection;
using EmailService.Application.Features.Listeners;
using EmailService.Domain.Settings;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService.Application;

public static class DependencyInjection
{
	extension(IServiceCollection services)
	{
		public IServiceCollection AddApplicationLayer(IConfiguration configuration)
		{
			services.AddOptions<MailSettings>()
				.Bind(configuration.GetSection(nameof(MailSettings)))
				.Validate(s => !string.IsNullOrWhiteSpace(s.From), "MailSettings.From is required.")
				.Validate(s => !string.IsNullOrWhiteSpace(s.DefaultEmailReceiver),
					"MailSettings.DefaultEmailReceiver is required.")
				.ValidateOnStart();
		
			var executingAssembly = Assembly.GetExecutingAssembly();
			services.AddMediator(AppDomain.CurrentDomain.GetAssemblies());

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
		
			services.AddValidatorsFromAssembly(executingAssembly);
		
			services.AddSingleton(TimeProvider.System);

			services.AddCommonHealthChecks(configuration);

			services.AddControllers();

			return services;
		}
	}
}