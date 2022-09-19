namespace IconService.Contracts.Icon;

public record CreateRequest
(
    string? Name,
    string? Description,
    string? Icon,
    bool DayIcon,
    byte[]? FileContent
);

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);

public record AuthenticationResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Token);

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<ErrorOr<AuthenticationResult>>;