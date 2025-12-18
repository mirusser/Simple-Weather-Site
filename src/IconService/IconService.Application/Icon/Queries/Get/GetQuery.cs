using Common.Mediator;
using ErrorOr;
using IconService.Application.Icon.Models.Dto;

namespace IconService.Application.Icon.Queries.Get;

public record GetQuery
(
    string? Icon
) : IRequest<ErrorOr<GetResult?>>;