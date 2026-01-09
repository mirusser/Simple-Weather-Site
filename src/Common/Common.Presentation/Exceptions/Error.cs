namespace Common.Presentation.Exceptions;

public readonly record struct Error(
    string Code,
    string Description,
    ErrorType Type = ErrorType.Failure,
    Dictionary<string, object>? Metadata = null);