using Common.Presentation.Controllers;
using HangfireService.Features.Commands;
using HangfireService.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HangfireService.Controllers;

public class JobController(ISender mediator) : ApiController
{
	[HttpPost]
	public async Task<IActionResult> RegisterJob(RegisterJobCommand request)
	{
		await mediator.Send(request);
		return Ok();
	}

	[HttpPost]
	public async Task<IActionResult> GetAllJobTypes(GetAllJobTypeQuery request)
	{
		await mediator.Send(request);
		return Ok();
	}
}