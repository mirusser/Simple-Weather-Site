using System.Reflection;
using Common.Application.Behaviors;
using Common.Application.HealthChecks;
using Common.Application.Mapping;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IconService.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
	{
		var executingAssembly = Assembly.GetExecutingAssembly();
		services.AddMappings(executingAssembly);

		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(executingAssembly));

		services.AddScoped(
			typeof(IPipelineBehavior<,>),
			typeof(ValidationBehavior<,>));

		services.AddValidatorsFromAssembly(executingAssembly);

		services.AddTransient(
			typeof(IPipelineBehavior<,>),
			typeof(LoggingBehavior<,>));

		services.AddCommonHealthChecks(configuration);

		return services;
	}
}