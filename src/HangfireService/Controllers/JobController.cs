using Common.Mediator;
using Common.Presentation.Controllers;
using HangfireService.Features.Commands;
using HangfireService.Features.Queries;
using Microsoft.AspNetCore.Mvc;

namespace HangfireService.Controllers;

public class JobController(IMediator mediator) : ApiController
{
	[HttpPost]
	public async Task<IActionResult> RegisterJob(RegisterJobCommand request)
	{
		await mediator.SendAsync(request);
		return Ok();
	}

	[HttpPost]
	public async Task<IActionResult> GetAllJobTypes(GetAllJobTypeQuery request)
	{
		await mediator.SendAsync(request);
		return Ok();
	}
}