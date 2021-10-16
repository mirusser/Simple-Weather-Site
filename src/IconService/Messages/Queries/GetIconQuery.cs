using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using IconService.Models.Dto;
using IconService.Mongo.Documents;
using IconService.Services;
using MediatR;
using MongoDB.Driver;

namespace IconService.Messages.Queries
{
    public class GetIconQuery : IRequest<IconDto?>
    {
        public string? Icon { get; set; }
    }

    public class GetIconQueryHandler : IRequestHandler<GetIconQuery, IconDto?>
    {
        private readonly IMongoCollection<IconDocument> _iconCollection;
        private readonly IMapper _mapper;

        public GetIconQueryHandler(
            IMongoCollectionFactory<IconDocument> mongoCollectionFactory,
            IMapper mapper)
        {
            _iconCollection = mongoCollectionFactory.Create();
            _mapper = mapper;
        }

        public async Task<IconDto?> Handle(GetIconQuery request, CancellationToken cancellationToken)
        {
            IconDto? iconDto = null;

            var iconDocument =
                await _iconCollection.Find(
                    i => i.Icon == request.Icon)
                .FirstOrDefaultAsync();

            if (iconDocument != null)
            {
                iconDto = _mapper.Map<IconDto?>(iconDocument);
            }

            return iconDto;
        }
    }
}