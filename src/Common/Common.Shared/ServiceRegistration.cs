using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.Shared;

public static class ServiceRegistration
{
	public static IServiceCollection AddSharedLayer(this IServiceCollection services, IConfiguration configuration)
	{
		// in .NET 9 and later you could use: JsonSerializerOptions.Default
		//https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/configure-options?pivots=dotnet-8-0
		services.AddSingleton<JsonSerializerOptions>(_ => new(JsonSerializerDefaults.Web));

		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();

		return services;
	}

	public static IApplicationBuilder UseDefaultSwagger(this IApplicationBuilder builder)
	{
		builder.UseSwagger();
		builder.UseSwaggerUI();

		return builder;
	}

	/// <summary>
	/// Page with basic environment information about the service
	/// </summary>
	public static IEndpointRouteBuilder UseServiceStartupPage(this IEndpointRouteBuilder app, IHostEnvironment environment)
	{
		app.MapGet("/", () => environment);

		return app;
	}
}