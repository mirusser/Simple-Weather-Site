namespace IconService.Application.Icon.Models.Dto;

public record CreateResult
(
    string Name,
    string Description,
    string Icon,
    bool DayIcon,
    byte[] FileContent
);