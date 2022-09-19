using ErrorOr;
using IconService.Application.Common.Interfaces.Persistence;
using IconService.Application.Icon.Models.Dto;
using IconService.Domain.Common.Errors;
using IconService.Domain.Entities.Documents;
using MapsterMapper;
using MediatR;

namespace IconService.Application.Icon.Commands.Create;

public class CreateCommandHandler
    : IRequestHandler<CreateCommand, ErrorOr<CreateResult>>
{
    private readonly IMapper _mapper;
    private readonly IMongoRepository<IconDocument> _iconRepository;

    public CreateCommandHandler(
        IMapper mapper,
        IMongoRepository<IconDocument> iconRepository)
    {
        _mapper = mapper;
        _iconRepository = iconRepository;
    }

    public async Task<ErrorOr<CreateResult>> Handle(
        CreateCommand request,
        CancellationToken cancellationToken)
    {
        var iconDocument = _mapper.Map<IconDocument>(request);
        iconDocument = await _iconRepository.CreateOneAsync(
            iconDocument,
            null,
            cancellationToken);

        if (iconDocument is null || iconDocument.Id == default)
        {
            return Errors.Icon.IconNotCreated;
        }

        var createIconDto = _mapper.Map<CreateResult>(iconDocument);

        return createIconDto;
    }
}