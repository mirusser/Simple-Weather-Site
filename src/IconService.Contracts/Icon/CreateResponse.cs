namespace IconService.Contracts.Icon;

public record CreateResponse
(
    string Name,
    string Description,
    string Icon,
    bool DayIcon,
    byte[] FileContent
);