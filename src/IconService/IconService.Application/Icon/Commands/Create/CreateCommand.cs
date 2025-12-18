using Common.Mediator;
using ErrorOr;
using IconService.Application.Icon.Models.Dto;

namespace IconService.Application.Icon.Commands.Create;

public record CreateCommand
(
     string? Name,
     string? Description,
     string? Icon,
     bool DayIcon,
     byte[]? FileContent
) : IRequest<ErrorOr<CreateResult>>;