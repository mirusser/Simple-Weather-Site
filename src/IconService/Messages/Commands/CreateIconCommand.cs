using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using IconService.Mongo.Documents;
using IconService.Services;
using MediatR;
using MongoDB.Driver;

namespace IconService.Messages.Commands
{
    public class CreateIconCommand : IRequest<bool>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public bool DayIcon { get; set; }
        public byte[]? FileContent { get; set; }
    }

    public class CreateIconHandler : IRequestHandler<CreateIconCommand, bool>
    {
        private readonly IMapper _mapper;
        private readonly IMongoCollection<IconDocument> _iconCollection;

        public CreateIconHandler(
            IMapper mapper,
            IMongoCollectionFactory<IconDocument> mongoCollectionFactory)
        {
            _mapper = mapper;
            _iconCollection = mongoCollectionFactory.Create();
        }

        public async Task<bool> Handle(CreateIconCommand request, CancellationToken cancellationToken)
        {
            var iconDocument = _mapper.Map<IconDocument>(request);
            await _iconCollection.InsertOneAsync(iconDocument);

            return true;
        }
    }
}
