namespace IconService.Contracts.Icon;

public record CreateRequest
(
    string? Name,
    string? Description,
    string? Icon,
    bool DayIcon,
    byte[]? FileContent
);