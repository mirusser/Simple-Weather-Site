namespace Common.Presentation.Exceptions;

public class ServiceException : Exception
{
    public string Code { get; init; }

    public ServiceException(
        string code,
        string message,
        params object[] args) : base(string.Format(message, args), null)
    {
        Code = code;
    }
    

    public sealed class NotFoundException(string message, string code = ErrorCodes.NotFound)
        : ServiceException(message, code);

    public sealed class ConflictException(string message, string code = ErrorCodes.Conflict)
        : ServiceException(message, code);

    public sealed class ForbiddenException(string message, string code = ErrorCodes.Forbidden)
        : ServiceException(message, code);

}