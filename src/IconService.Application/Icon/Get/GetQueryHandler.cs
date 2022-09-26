using ErrorOr;
using IconService.Application.Common.Interfaces.Persistence;
using IconService.Application.Icon.Models.Dto;
using IconService.Domain.Common.Errors;
using IconService.Domain.Entities.Documents;
using MapsterMapper;
using MediatR;

namespace IconService.Application.Icon.Get
{
    public class GetQueryHandler
        : IRequestHandler<GetQuery, ErrorOr<GetResult?>>
    {
        private readonly IMongoRepository<IconDocument> _iconRepository;
        private readonly IMapper _mapper;

        public GetQueryHandler(
            IMongoRepository<IconDocument> iconRepository,
            IMapper mapper)
        {
            _iconRepository = iconRepository;
            _mapper = mapper;
        }

        public async Task<ErrorOr<GetResult?>> Handle(
            GetQuery request,
            CancellationToken cancellationToken)
        {
            var iconDocument =
                await _iconRepository.FindOneAsync(
                    i => i.Icon == request.Icon,
                    findOptions: null,
                    cancellation: cancellationToken);

            if (iconDocument != null)
            {
                var iconDto = _mapper.Map<GetResult?>(iconDocument);
                return iconDto;
            }

            return Errors.Icon.IconNotFound;
        }
    }
}
