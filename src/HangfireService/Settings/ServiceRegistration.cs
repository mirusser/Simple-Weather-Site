using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using HangfireService.Features.Filters;
using MassTransit;

namespace HangfireService.Settings;

public static class ServiceRegistration
{
	extension(IServiceCollection services)
	{
		public IServiceCollection AddHangfireServices(IConfiguration configuration)
		{
			//StackOverflow post on how to register hangfire with mongo:
			//https://stackoverflow.com/questions/58340247/how-to-use-hangfire-in-net-core-with-mongodb

			MongoSettings mongoSettings = new();
			configuration.GetSection(nameof(MongoSettings)).Bind(mongoSettings);

			var migrationOptions = new MongoMigrationOptions
			{
				MigrationStrategy = new MigrateMongoMigrationStrategy(),
				BackupStrategy = new CollectionMongoBackupStrategy()
			};

		services.AddHangfire((sp, config) =>
		{
			config.UseFilter(sp.GetRequiredService<PerformContextAccessorFilter>());
			config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
			config.UseSimpleAssemblyNameTypeSerializer();
			config.UseRecommendedSerializerSettings();
			config.UseMongoStorage(
				mongoSettings.ConnectionString,
					mongoSettings.Database,
					new MongoStorageOptions
					{
						MigrationOptions = migrationOptions,
						CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection,
						QueuePollInterval = TimeSpan.FromSeconds(1)
					});
		});
		services.AddHangfireServer();

		services.AddSingleton<PerformContextAccessorFilter>();

			services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
			return services;
		}

		public IServiceCollection AddCustomMassTransit(IConfiguration configuration)
		{
			services.AddMassTransit(config =>
			{
				RabbitMQSettings rabbitMQSettings = new();
				configuration
					.GetSection(nameof(RabbitMQSettings))
					.Bind(rabbitMQSettings);

				config.SetKebabCaseEndpointNameFormatter();
				config.UsingRabbitMq((ctx, cfg) =>
				{
					cfg.Host(rabbitMQSettings.Host);
					cfg.ConfigureEndpoints(ctx);
				});
			});

			services.AddOptions<MassTransitHostOptions>()
				.Configure(options => options.WaitUntilStarted = true);

			return services;
		}
	}
}
