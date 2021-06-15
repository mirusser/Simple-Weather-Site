using AutoMapper;
using Convey.CQRS.Queries;
using IconService.Models.Dto;
using IconService.Mongo.Documents;
using IconService.Services;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IconService.Messages.Queries
{
    public class GetIconQuery : IQuery<IconDto>
    {
        public string Description { get; set; }
        public bool DayIcon { get; set; }
    }

    public class GetIconQueryHandler : IQueryHandler<GetIconQuery, IconDto>
    {
        private readonly IIconService _iconService;
        private readonly IMapper _mapper;

        public GetIconQueryHandler(
            IIconService iconService,
            IMapper mapper)
        {
            _iconService = iconService;
            _mapper = mapper;
        }

        public async Task<IconDto> HandleAsync(GetIconQuery query)
        {
            var iconDocument =
                await _iconService.GetAsync(query.Description, query.DayIcon);

            var iconDto = _mapper.Map<IconDto>(iconDocument);

            return iconDto;
        }
    }
}
