using ErrorOr;
using IconService.Application.Icon.Models.Dto;
using MediatR;

namespace IconService.Application.Icon.Get;

public record GetQuery
(
    string? Icon
): IRequest<ErrorOr<GetResult?>>;