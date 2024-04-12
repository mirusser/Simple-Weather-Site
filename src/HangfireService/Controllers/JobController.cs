using HangfireService.Features.Commands;
using HangfireService.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HangfireService.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class JobController(IMediator mediatr) : ControllerBase
{
	[HttpPost]
	public async Task<IActionResult> RegisterJob(RegisterJobCommand request)
	{
		await mediatr.Send(request);
		return Ok();
	}

	[HttpPost]
	public async Task<IActionResult> GetAllJobTypes(GetAllJobTypeQuery request)
	{
		await mediatr.Send(request);
		return Ok();
	}
}