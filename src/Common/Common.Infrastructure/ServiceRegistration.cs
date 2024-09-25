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

		return services;
	}
}