using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.HealthChecks;

//TODO: move magic strings to appsettings
public static class ServiceRegistration
{
	public static IHealthChecksBuilder AddCommonHealthChecks(this IServiceCollection services)
	{
		var executingAssemblyName = ExtensionMethods.AssemblyExtensions.GetProjectName();

		var builder = services.AddHealthChecks();

		services
			.AddHealthChecksUI(setup =>
			{
				setup.SetEvaluationTimeInSeconds(500);
				setup.AddHealthCheckEndpoint(executingAssemblyName, "/health");
			})
			.AddInMemoryStorage();

		return builder;
	}

	public static IApplicationBuilder UseCommonHealthChecks(this IApplicationBuilder app)
	{
		app.UseHealthChecks("/health", new HealthCheckOptions
		{
			Predicate = _ => true,
			ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
		});

		app.UseHealthChecksUI(options =>
		{
			options.UIPath = "/health-ui";
		});

		return app;
	}
}