using Common.Mediator;
using Common.Presentation.Http;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace Common.Presentation.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public abstract class ApiController : ControllerBase
{
    // Properties must be public (with public setter)
    // to be properly injected when using [FromServices] attribute 
    [FromServices]
    public IMediator Mediator { get; set; } = null!;

    [FromServices]
    public IMapper Mapper { get; set; } = null!;
    
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
    
    protected IActionResult FromResult<TIn, TOut>(Result<TIn> result)
        => result.Match<IActionResult>(
            onOk: value => Ok(Mapper.Map<TOut>(value)),
            onFail: problem => StatusCode(problem.Status, problem)
        );
}