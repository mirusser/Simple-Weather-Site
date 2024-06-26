﻿using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Domain.Settings;

namespace Common.Application.HealthChecks;

//TODO: move magic strings to appsettings
public static class ServiceRegistration
{
	public static IHealthChecksBuilder AddCommonHealthChecks(this IServiceCollection services, IConfiguration configuration)
	{
		var baseUrl = configuration
			.GetSection(nameof(ServiceEndpoints))
			.GetValue<string>(nameof(ServiceEndpoints.BaseUrl));

		var executingAssemblyName = ExtensionMethods.AssemblyExtensions.GetProjectName();

		var builder = services.AddHealthChecks();

		return builder;
	}

	public static IApplicationBuilder UseCommonHealthChecks(this IApplicationBuilder app)
	{
		app.UseHealthChecks("/health", new HealthCheckOptions
		{
			Predicate = _ => true,
			ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
		});

		return app;
	}
}