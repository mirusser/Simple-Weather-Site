using ErrorOr;
using IconService.Application.Icon.Models.Dto;
using MediatR;

namespace IconService.Application.Icon.GetAll;

public record GetAllQuery() : IRequest<ErrorOr<IEnumerable<GetResult>>>;