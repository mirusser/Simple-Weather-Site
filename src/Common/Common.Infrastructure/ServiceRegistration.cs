using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Infrastructure.Settings;

namespace Common.Infrastructure;

public static class ServiceRegistration
{
	public static IServiceCollection AddCommonInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		//TODO: remove later in memory cache
		services.AddMemoryCache();

		services.AddStackExchangeRedisCache(options =>
		{
			options.Configuration = configuration
				.GetSection(nameof(ConnectionStrings))
				.GetValue<string>(nameof(ConnectionStrings.RedisConnection));
		});

		return services;
	}
}