using System.Threading.Tasks;
using AutoMapper;
using Convey.CQRS.Queries;
using IconService.Models.Dto;
using IconService.Mongo.Documents;
using IconService.Services;
using MongoDB.Driver;

namespace IconService.Messages.Queries
{
    public class GetIconQuery : IQuery<IconDto?>
    {
        public string? Icon { get; set; }
    }

    public class GetIconQueryHandler : IQueryHandler<GetIconQuery, IconDto?>
    {
        private readonly IMongoCollection<IconDocument> _iconCollection;
        private readonly IMapper _mapper;

        public GetIconQueryHandler(
            IMongoCollection<IconDocument> iconCollection,
            IMapper mapper)
        {
            _iconCollection = iconCollection;
            _mapper = mapper;
        }

        public async Task<IconDto?> HandleAsync(GetIconQuery query)
        {
            IconDto? iconDto = null;

            var iconDocument =
                (await _iconCollection.FindAsync(i => i.Icon == query.Icon)).FirstOrDefault();

            if (iconDocument != null)
            {
                iconDto = _mapper.Map<IconDto?>(iconDocument);
            }

            return iconDto;
        }
    }
}