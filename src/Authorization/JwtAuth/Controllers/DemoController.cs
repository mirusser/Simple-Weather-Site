using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuth.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class DemoController(AuthenticationManager authenticationManager) : ControllerBase
{
	[Authorize]
	[HttpGet]
	public IResult Get()
	{
		return Results.Ok();
	}

	[AllowAnonymous]
	[HttpPost]
	public IResult Authorize([FromBody] UserRequest user)
	{
		if (authenticationManager.TryAuthenticate(user.Username, user.Password, out var token))
		{
			return Results.Ok(token);
		}

		return Results.Unauthorized();
	}
}

public class UserRequest
{
	public string Username { get; set; }
	public string Password { get; set; }
}