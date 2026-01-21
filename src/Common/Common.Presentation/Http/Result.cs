using Common.Domain.Errors;
using Microsoft.AspNetCore.Http;

namespace Common.Presentation.Http;

public readonly record struct Result<T>(T? Value, Problem? Problem)
{
    public bool IsSuccess => Problem is null;

    public static Result<T> Ok(T value) => new(value, null);
    public static Result<T> Fail(Problem problem) => new(default, problem);

    public TResult Match<TResult>(Func<T, TResult> onOk, Func<Problem, TResult> onFail)
        => IsSuccess ? onOk(Value!) : onFail(Problem!);
}

public readonly record struct Result(Problem? Problem)
{
    public bool IsSuccess => Problem is null;
    public static Result Ok() => new(null);
    public static Result Fail(Problem problem) => new(problem);
    public TResult Match<TResult>(Func<TResult> onOk, Func<Problem, TResult> onFail)
        => IsSuccess ? onOk() : onFail(Problem!);
}

public sealed record Problem(string Code, string Message, int Status);

public static class Problems
{
    public static Problem Validation(string message = "Validation failed")
        => new(ErrorCodes.Validation, message, StatusCodes.Status400BadRequest);

    public static Problem NotFound(string message)
        => new(ErrorCodes.NotFound, message, StatusCodes.Status404NotFound);

    public static Problem Conflict(string message)
        => new(ErrorCodes.Conflict, message, StatusCodes.Status409Conflict);

    public static Problem Unauthorized(string message = "Unauthorized")
        => new(ErrorCodes.Unauthorized, message, StatusCodes.Status401Unauthorized);

    public static Problem ServiceUnavailable(string message = "Service unavailable")
        => new(ErrorCodes.ServiceUnavailable, message, StatusCodes.Status503ServiceUnavailable);
    
    public static Problem BadGateway(string message = "Bad gateway")
        => new(ErrorCodes.BadGateway, message, StatusCodes.Status502BadGateway);

    public static Problem GatewayTimeout(string message = "Gateway timeout")
        => new(ErrorCodes.GatewayTimeout, message, StatusCodes.Status504GatewayTimeout);
}

