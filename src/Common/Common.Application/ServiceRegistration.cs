using System.Text.Json;
using Common.Contracts.HealthCheck;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.Application;

public static class ServiceRegistration
{
	public static IApplicationBuilder UseCommonHealthChecks(this IApplicationBuilder app)
	{
		app.UseHealthChecks("/health", new HealthCheckOptions
		{
			ResponseWriter = responseWriter
		});

		return app;
	}

	private static readonly Func<HttpContext, HealthReport, Task> responseWriter = async (context, report) =>
	{
		context.Response.ContentType = "application/json";
		var response = new HealthCheckResponse(
			Status: report.Status.ToString(),
			HealthChecks: report.Entries.Select(x => new IndividualHealthCheckResponse(
				Status: x.Value.Status.ToString(),
				Component: x.Key,
				Description: x.Value.Description,
				Exception: x.Value.Exception)),
			HealthCheckDuration: report.TotalDuration);
		await context.Response.WriteAsync(JsonSerializer.Serialize(response)).ConfigureAwait(false);
	};
}