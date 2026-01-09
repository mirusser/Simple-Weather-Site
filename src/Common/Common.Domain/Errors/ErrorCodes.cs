namespace Common.Domain.Errors;

/// <summary>
/// It's a partial class because it could be
/// expanded in distinct services for their custom error codes
/// </summary>
public static partial class ErrorCodes
{
    public const string DefaultError = "error";

    public const string Validation = "validation";

    public const string ServiceUnavailable = "service_unavailable";

    public const string Unauthorized = "unauthorized";

    public const string NotFound = "not_found";

    public const string Conflict = "conflict";

    public const string Failure = "failure";

    public const string Unexpected = "unexpected";

    public const string Forbidden = "forbidden";
}