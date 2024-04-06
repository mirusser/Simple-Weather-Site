using ErrorOr;
using IconService.Application.Icon.Models.Dto;
using MediatR;

namespace IconService.Application.Icon.Queries.Get;

public record GetQuery
(
    string? Icon
) : IRequest<ErrorOr<GetResult?>>;