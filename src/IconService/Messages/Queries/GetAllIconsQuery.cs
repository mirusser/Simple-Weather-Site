using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using IconService.Models.Dto;
using IconService.Mongo.Documents;
using IconService.Mongo.Repository;
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
        private readonly IMongoRepository<IconDocument> _iconRepository;

        public GetAllIconsHandler(IMapper mapper, IMongoRepository<IconDocument> iconRepository)
        {
            _mapper = mapper;
            _iconRepository = iconRepository;
        }

        public async Task<IEnumerable<GetIconDto>> Handle(GetAllIconsQuery request, CancellationToken cancellationToken)
        {
            var iconDocuments = await _iconRepository.GetAllAsync(cancellation: cancellationToken);
            return _mapper.Map<IEnumerable<GetIconDto>>(iconDocuments);
        }
    }
}