using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Infrastructure.Settings;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Managers;
using Polly;
using Polly.Retry;

namespace Common.Infrastructure;

public static class ServiceRegistration
{
	extension(IServiceCollection services)
	{
		public IServiceCollection AddCommonInfrastructure(IConfiguration configuration)
		{
			services.AddStackExchangeRedisCache(options =>
			{
				options.Configuration = configuration
					.GetSection(nameof(ConnectionStrings))
					.GetValue<string>(nameof(ConnectionStrings.RedisConnection));
			});

			services.AddSingleton<ICacheManager, CacheManager>();

			services.AddHttpClient();
			
			services.AddCommonResiliencePipelines(configuration);
			services.AddHttpExecutor();
		
			return services;
		}

		public IServiceCollection AddCommonResiliencePipelines(IConfiguration configuration)
		{
			var defaultPipe = new ResiliencePipelineOptions();
			configuration
				.GetSection($"{nameof(ResiliencePipelines)}:{nameof(ResiliencePipelines.Default)}")
				.Bind(defaultPipe);

			services.AddResiliencePipeline(defaultPipe.Name, pipeline =>
			{
				// Retry with exponential backoff + jitter
				pipeline.AddRetry(new RetryStrategyOptions
				{
					MaxRetryAttempts = defaultPipe.MaxRetryAttempts,
					Delay = TimeSpan.FromSeconds(defaultPipe.DelaySeconds),
					BackoffType = DelayBackoffType.Exponential,
					UseJitter = defaultPipe.UseJitter,
					// more potential configuration here
				});
				
				// Add a simple timeout around the whole HTTP call
				pipeline.AddTimeout(TimeSpan.FromSeconds(defaultPipe.TimeoutSeconds));
			});

			var healthPipe = new ResiliencePipelineOptions();
			configuration
				.GetSection($"{nameof(ResiliencePipelines)}:{nameof(ResiliencePipelines.Health)}")
				.Bind(healthPipe);

			services.AddResiliencePipeline(healthPipe.Name, pipeline =>
			{
				// Typically: NO retry, just a short timeout.
				pipeline.AddTimeout(TimeSpan.FromSeconds(healthPipe.TimeoutSeconds));
			});
			
			return services;
		}

		public IServiceCollection AddHttpExecutor()
		{
			services.AddSingleton<IHttpExecutor, HttpExecutor>();
			services.AddSingleton<IHttpRequestFactory, HttpRequestFactory>();
			
			return services;
		}
	}
}
