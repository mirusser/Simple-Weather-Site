using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Infrastructure.Settings;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Managers;
using Polly;
using Polly.Retry;
using ResiliencePipeline = Common.Infrastructure.Settings.ResiliencePipeline;

namespace Common.Infrastructure;

public static class ServiceRegistration
{
	public static IServiceCollection AddCommonInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddStackExchangeRedisCache(options =>
		{
			options.Configuration = configuration
				.GetSection(nameof(ConnectionStrings))
				.GetValue<string>(nameof(ConnectionStrings.RedisConnection));
		});

		services.AddSingleton<ICacheManager, CacheManager>();

		services.AddHttpClient();
		
		ResiliencePipeline resiliencePipelineSettings = new();
		configuration
			.GetSection(nameof(ResiliencePipeline))
			.Bind(resiliencePipelineSettings);
		
		services.AddResiliencePipeline(resiliencePipelineSettings.Name, pipelineBuilder =>
		{
			// Retry with exponential backoff + jitter
			pipelineBuilder.AddRetry(new RetryStrategyOptions
			{
				MaxRetryAttempts = resiliencePipelineSettings.MaxRetryAttempts,
				Delay = TimeSpan.FromSeconds(resiliencePipelineSettings.DelaySeconds),
				BackoffType = DelayBackoffType.Exponential,
				UseJitter = resiliencePipelineSettings.UseJitter,
				// more potential configuration here
			});

			// Add a simple timeout around the whole HTTP call
			pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(resiliencePipelineSettings.DefaultTimeoutSeconds));
		});
		
		services.AddSingleton<IHttpExecutor, HttpExecutor>();
		
		return services;
	}
}