using Common.Domain.Errors;
using Common.Mediator;
using IconService.Application.Common.Interfaces.Persistence;
using IconService.Application.Icon.Models.Dto;
using IconService.Domain.Common.Errors;
using IconService.Domain.Entities.Documents;
using MapsterMapper;

namespace IconService.Application.Icon.Queries.Get;

public record GetQuery
(
    string? Icon
) : IRequest<GetResult?>;

public class GetQueryHandler(
    IMongoRepository<IconDocument> iconRepository,
    IMapper mapper)
    : IRequestHandler<GetQuery, GetResult?>
{
    public async Task<GetResult?> Handle(
        GetQuery request,
        CancellationToken cancellationToken)
    {
        var iconDocument =
            await iconRepository.FindOneAsync(
                i => i.Icon == request.Icon,
                findOptions: null,
                cancellation: cancellationToken);

        if (iconDocument != null)
        {
            var iconDto = mapper.Map<GetResult?>(iconDocument);
            return iconDto;
        }

        throw new ServiceException.NotFoundException(
            code: Errors.Icon.IconNotFound.Code,
            message: Errors.Icon.IconNotFound.Description);
    }
}