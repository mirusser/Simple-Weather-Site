using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using IconService.Models.Dto;
using IconService.Mongo.Documents;
using IconService.Mongo.Repository;
using IconService.Services;
using MediatR;
using MongoDB.Driver;

namespace IconService.Messages.Commands
{
    public class CreateIconCommand : IRequest<CreateIconDto>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public bool DayIcon { get; set; }
        public byte[]? FileContent { get; set; }
    }

    public class CreateIconHandler : IRequestHandler<CreateIconCommand, CreateIconDto>
    {
        private readonly IMapper _mapper;
        private readonly IMongoRepository<IconDocument> _iconRepository;

        public CreateIconHandler(
            IMapper mapper, IMongoRepository<IconDocument> iconRepository)
        {
            _mapper = mapper;
            _iconRepository = iconRepository;
        }

        public async Task<CreateIconDto> Handle(CreateIconCommand request, CancellationToken cancellationToken)
        {
            var iconDocument = _mapper.Map<IconDocument>(request);
            iconDocument = await _iconRepository.CreateOneAsync(iconDocument, null, cancellationToken);

            var createIconDto = _mapper.Map<CreateIconDto>(iconDocument);

            return createIconDto;
        }
    }
}
