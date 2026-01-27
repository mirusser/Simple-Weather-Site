using System.Reflection;
using Common.Application.HealthChecks;
using Common.Application.Mapping;
using Common.Mediator.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IconService.Application;

public static class DependencyInjection
{
	extension(IServiceCollection services)
	{
		public IServiceCollection AddApplication(IConfiguration configuration)
		{
			var executingAssembly = Assembly.GetExecutingAssembly();
			services.AddMappings(executingAssembly);

			services.AddValidatorsFromAssembly(executingAssembly);
			services.AddMediator(AppDomain.CurrentDomain.GetAssemblies());

			services.AddCommonHealthChecks(configuration);

			return services;
		}
	}
}