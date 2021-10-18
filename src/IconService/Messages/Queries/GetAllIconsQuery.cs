using System.Collections.Generic;
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
    public class GetAllIconsQuery : IRequest<IEnumerable<GetIconDto>>
    {
    }

    public class GetAllIconsHandler : IRequestHandler<GetAllIconsQuery, IEnumerable<GetIconDto>>
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<IconDocument> _iconCollection;

        public GetAllIconsHandler(IMapper mapper, IMongoCollectionFactory<IconDocument> mongoCollectionFactory)
        {
            _mapper = mapper;
            _iconCollection = mongoCollectionFactory.Create();
        }

        public async Task<IEnumerable<GetIconDto>> Handle(GetAllIconsQuery request, CancellationToken cancellationToken)
        {
            var iconDocuments = (await _iconCollection.FindAsync(_ => true, cancellationToken: cancellationToken)).ToEnumerable(cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<GetIconDto>>(iconDocuments);
        }
    }
}