using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace HangfireService.Features.Filters;

public class AuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize([NotNull] DashboardContext context)
	{
		// TODO: Implement authorization (and authentication) logic
		//return context.GetHttpContext().User.Identity.IsAuthenticated;
		return true;
	}
}
