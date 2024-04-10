using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Common.Contracts.HealthCheck;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Common.Application;

public static class DependencyInjection
{
	public static IApplicationBuilder UseCommonHealthChecks(this IApplicationBuilder app)
	{
		app.UseHealthChecks("/health", new HealthCheckOptions
		{
			ResponseWriter = async (context, report) =>
			{
				context.Response.ContentType = "application/json";
				var response = new HealthCheckResponse
				{
					Status = report.Status.ToString(),
					HealthCheckDuration = report.TotalDuration,
					HealthChecks = report.Entries.Select(x => new IndividualHealthCheckResponse
					{
						Component = x.Key,
						Status = x.Value.Status.ToString(),
						Description = x.Value.Description
					})
				};
				await context.Response.WriteAsync(JsonSerializer.Serialize(response));
			}
		});

		return app;
	}
}