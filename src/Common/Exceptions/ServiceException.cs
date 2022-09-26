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
}