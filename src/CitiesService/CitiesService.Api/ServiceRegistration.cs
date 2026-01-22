using System;
using System.Net;
using CitiesService.Application.Features.HealthChecks;
using CitiesService.Infrastructure.Contexts;
using Common.Application.HealthChecks;
using Common.Contracts.HealthCheck;
using Common.Presentation.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace CitiesService.Api;

// TODO: fix magic strings
public static class ServiceRegistration
{
	public static IWebHostBuilder CustomKestrelConfiguration(
		this IWebHostBuilder hostBuilder,
		IHostEnvironment environment)
	{
		if (environment.EnvironmentName == "Docker")
		{
			hostBuilder.ConfigureKestrel(serverOptions =>
			{
				// Configure Kestrel to use HTTPS
				serverOptions.Listen(IPAddress.Any, 443, listenOptions =>
				{
					listenOptions.UseHttps("cert/localhost.pfx", "zaq1@WSX"); //use your_pfx_password
				});

				// Configure Kestrel to use HTTP on port 80
				serverOptions.Listen(IPAddress.Any, 80);
			});
		}

		return hostBuilder;
	}

	public static IServiceCollection AddPresentation(
		this IServiceCollection services,
		IConfiguration configuration,
		IHostEnvironment environment)
	{
		services.AddControllers();
		services
			.AddCors(options =>
			{
				options.AddPolicy("AllowAll",
					builder =>
					{
						builder
						.AllowAnyOrigin()
						.AllowAnyMethod()
						.AllowAnyHeader();
					});
			})
			.AddHttpClient()
			.AddCustomAuth(configuration, environment);

		services
			.AddCommonHealthChecks()
			.AddDbContextCheck<ApplicationDbContext>(
				name: "SQL health check",
				failureStatus: HealthStatus.Unhealthy,
				tags: [nameof(HealthChecksTags.Database)])
			.AddCheck<CitiesAvailableHealthCheck>(
				name: "Cities available health check",
				failureStatus: HealthStatus.Degraded,
				tags: [nameof(HealthChecksTags.Database)]);

		return services;
	}

	private static IServiceCollection AddCustomAuth(
		this IServiceCollection services,
		IConfiguration configuration,
		IHostEnvironment environment)
	{
		ApiResourceAuthSettings apiResourceAuthSettings = new();
		configuration
			.GetSection(nameof(ApiResourceAuthSettings))
			.Bind(apiResourceAuthSettings);

		services
			.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
			{
				options.Authority = apiResourceAuthSettings.AuthorityUrl;
				options.Audience = apiResourceAuthSettings.Audience;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateAudience = true,
					ValidateIssuer = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ClockSkew = TimeSpan.Zero
				};

				if (!environment.IsProduction())
				{
					options.RequireHttpsMetadata = false;
				}
			});

		services.AddAuthorizationBuilder()
			.AddPolicy("ApiScope", policy =>
			{
				policy.RequireAuthenticatedUser();
				foreach (var claim in apiResourceAuthSettings.RequiredClaims)
				{
					foreach (var allowedValue in claim.AllowedValues)
					{
						policy.RequireClaim(claim.ClaimType, allowedValue);
					}
				}
			});

		return services;
	}
}