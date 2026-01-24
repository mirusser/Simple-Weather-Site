using Common.Domain.Errors;
using Common.Mediator;
using Common.Presentation.Exceptions;
using IconService.Application.Common.Interfaces.Persistence;
using IconService.Application.Icon.Models.Dto;
using IconService.Domain.Common.Errors;
using IconService.Domain.Entities.Documents;
using MapsterMapper;

namespace IconService.Application.Icon.Commands.Create;

public record CreateCommand
(
    string? Name,
    string? Description,
    string? Icon,
    bool DayIcon,
    byte[]? FileContent
) : IRequest<CreateResult>;

public class CreateCommandHandler(
    IMapper mapper,
    IMongoRepository<IconDocument> iconRepository)
    : IRequestHandler<CreateCommand, CreateResult>
{
    public async Task<CreateResult> Handle(
        CreateCommand request,
        CancellationToken cancellationToken)
    {
        var iconDocument = mapper.Map<IconDocument>(request);
        iconDocument = await iconRepository.CreateOneAsync(
            iconDocument,
            null,
            cancellationToken);

        if (iconDocument?.Id == null)
        {
            throw new ServiceException(
                code: Errors.Icon.IconNotCreated.Code, 
                message:Errors.Icon.IconNotCreated.Description);
        }

        var createIconDto = mapper.Map<CreateResult>(iconDocument);

        return createIconDto;
    }
}