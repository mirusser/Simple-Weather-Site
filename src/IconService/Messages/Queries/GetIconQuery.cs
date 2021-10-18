using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using IconService.Models.Dto;
using IconService.Mongo.Documents;
using IconService.Mongo.Repository;
using MediatR;

namespace IconService.Messages.Queries
{
    public class GetIconQuery : IRequest<GetIconDto?>
    {
        public string? Icon { get; set; }
    }

    public class GetIconQueryHandler : IRequestHandler<GetIconQuery, GetIconDto?>
    {
        private readonly IMongoRepository<IconDocument> _iconRepository;
        private readonly IMapper _mapper;

        public GetIconQueryHandler(
            IMongoRepository<IconDocument> iconRepository,
            IMapper mapper)
        {
            _iconRepository = iconRepository;
            _mapper = mapper;
        }

        public async Task<GetIconDto?> Handle(GetIconQuery request, CancellationToken cancellationToken)
        {
            GetIconDto? iconDto = null;

            var iconDocument =
                await _iconRepository
                .FindOneAsync(i => i.Icon == request.Icon, cancellation: cancellationToken);

            if (iconDocument != null)
            {
                iconDto = _mapper.Map<GetIconDto?>(iconDocument);
            }

            return iconDto;
        }
    }
}