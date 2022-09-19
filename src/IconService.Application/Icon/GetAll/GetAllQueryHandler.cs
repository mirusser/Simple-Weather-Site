using ErrorOr;
using IconService.Application.Common.Interfaces.Persistence;
using IconService.Application.Icon.Models.Dto;
using IconService.Domain.Common.Errors;
using IconService.Domain.Entities.Documents;
using MapsterMapper;
using MediatR;

namespace IconService.Application.Icon.GetAll;

public class GetAllIconsHandler
    : IRequestHandler<GetAllQuery, ErrorOr<IEnumerable<GetResult>>>
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

    public async Task<ErrorOr<IEnumerable<GetResult>>> Handle(
        GetAllQuery request,
        CancellationToken cancellationToken)
    {
        var iconDocuments = await _iconRepository.GetAllAsync(cancellation: cancellationToken);

        if (iconDocuments != null)
        {
            var getIconDtos = _mapper.Map<IEnumerable<GetResult>>(iconDocuments);

            // the library ErrorOr force to make it to list (?)
            return getIconDtos.ToList();
        }

        return Errors.Icon.IconNotFound;
    }
}