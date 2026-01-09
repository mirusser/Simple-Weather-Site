using Common.Domain.Errors;
using Common.Mediator;
using Common.Presentation.Exceptions;
using IconService.Application.Common.Interfaces.Persistence;
using IconService.Application.Icon.Models.Dto;
using IconService.Domain.Common.Errors;
using IconService.Domain.Entities.Documents;
using MapsterMapper;

namespace IconService.Application.Icon.Queries.GetAll;

public record GetAllQuery() : IRequest<IEnumerable<GetResult>>;

public class GetAllIconsHandler
    : IRequestHandler<GetAllQuery, IEnumerable<GetResult>>
{
    private readonly IMapper _mapper;
    private readonly IMongoRepository<IconDocument> _iconRepository;

    public GetAllIconsHandler(
        IMapper mapper,
        IMongoRepository<IconDocument> iconRepository)
    {
        _mapper = mapper;
        _iconRepository = iconRepository;
    }

    public async Task<IEnumerable<GetResult>> Handle(
        GetAllQuery request,
        CancellationToken cancellationToken)
    {
        var iconDocuments = await _iconRepository
            .GetAllAsync(cancellation: cancellationToken);

        if (iconDocuments is not null)
        {
            var getIconDtos = _mapper.Map<IEnumerable<GetResult>>(iconDocuments);

            // the library ErrorOr force to make it to list (?)
            return getIconDtos;
        }

        throw new ServiceException.NotFoundException(
            code: Errors.Icon.IconNotFound.Code,
            message:Errors.Icon.IconNotFound.Description);
    }
}