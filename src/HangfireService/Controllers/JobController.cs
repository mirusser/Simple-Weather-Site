using Common.Presentation.Controllers;
using HangfireService.Features.Commands;
using HangfireService.Features.Queries;
using Microsoft.AspNetCore.Mvc;

namespace HangfireService.Controllers;

public class JobController : ApiController
{
	[HttpPost]
	public async Task<IActionResult> RegisterJob(RegisterJobCommand request)
	{
		await Mediator.SendAsync(request);
		
		return Ok();
	}

	[HttpPost]
	public async Task<IActionResult> GetAllJobTypes(GetAllJobTypeQuery request)
	{
		await Mediator.SendAsync(request);
		
		return Ok();
	}
}