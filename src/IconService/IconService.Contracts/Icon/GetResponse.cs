namespace IconService.Contracts.Icon;
public record GetResponse
(
    string Id,
    string Name,
    string Description,
    string Icon,
    bool DayIcon,
    byte[] FileContent
);