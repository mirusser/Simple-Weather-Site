using Common.Presentation.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.Presentation.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ApiController : ControllerBase
{
    protected IActionResult FromResult<T>(Result<T> result)
        => result.Match<IActionResult>(
            onOk: value => Ok(value),
            onFail: problem => StatusCode(problem.Status, problem)
        );
    
    protected IActionResult FromResult<T>(Result<T> result, Func<T, object> mapOk)
        => result.Match<IActionResult>(
            onOk: value => Ok(mapOk(value)),
            onFail: problem => StatusCode(problem.Status, problem)
        );
}