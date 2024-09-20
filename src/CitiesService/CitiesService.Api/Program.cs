using System;
using CitiesService.Api;
using CitiesService.Application;
using CitiesService.Infrastructure;
using Common.Presentation;
using Common.Presentation.Settings;
using Common.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
{
	builder.Host.UseSerilog();

	builder.Services
		.AddInfrastructure(builder.Configuration)
		.AddApplicationLayer(builder.Configuration)
		.AddCommonPresentationLayer(builder.Configuration)
		.AddPresentation(builder.Configuration);

	ApiResourceAuthSettings apiResourceAuthSettings = new();
	builder.Configuration
		.GetSection(nameof(ApiResourceAuthSettings))
		.Bind(apiResourceAuthSettings);

	builder.Services
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
			options.RequireHttpsMetadata = false;
		});

	builder.Services.AddAuthorizationBuilder()
		.AddPolicy("ApiScope", policy =>
		{
			policy.RequireAuthenticatedUser();
			foreach(var claim in apiResourceAuthSettings.RequiredClaims)
			{
				foreach(var allowedValue in claim.AllowedValues)
				{
					policy.RequireClaim(claim.ClaimType, allowedValue);
				}
			}
		});
}

var app = builder.Build();
{
	app
	.UseStaticFiles() // TODO: remove after this issue is fixed: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2130 (also wwwroot directory)
	.UseDefaultExceptionHandler()
	.UseDefaultSwagger()
	//.UseHttpsRedirection() // TODO: Redirect deletes authorization header - figure out/apply the solution https://stackoverflow.com/questions/28564961/authorization-header-is-lost-on-redirect
	.UseRouting()
	.UseAuthentication()
	.UseAuthorization()
	.UseCors("AllowAll")
	.UseApplicationLayer(builder.Configuration);

	app.MapControllers();
	app.UseServiceStartupPage(builder.Environment);
}

await app.RunWithLoggerAsync();