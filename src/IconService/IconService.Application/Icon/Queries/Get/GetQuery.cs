using Common.Mediator;
using IconService.Application.Icon.Models.Dto;

namespace IconService.Application.Icon.Queries.Get;

public record GetQuery
(
    string? Icon
) : IRequest<GetResult?>;