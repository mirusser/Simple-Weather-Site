using ErrorOr;
using IconService.Application.Icon.Models.Dto;
using MediatR;

namespace IconService.Application.Icon.Queries.GetAll;

public record GetAllQuery() : IRequest<ErrorOr<IEnumerable<GetResult>>>;