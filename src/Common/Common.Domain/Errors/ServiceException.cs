namespace Common.Domain.Errors;

public class ServiceException(
    string code,
    string message,
    params object[] args) : Exception(string.Format(message, args), null)
{
    public string Code { get; init; } = code;


    public sealed class NotFoundException(string message, string code = ErrorCodes.NotFound)
        : ServiceException(code, message);

    public sealed class ConflictException(string message, string code = ErrorCodes.Conflict)
        : ServiceException(code, message);

    public sealed class ForbiddenException(string message, string code = ErrorCodes.Forbidden)
        : ServiceException(code, message);

}