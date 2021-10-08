using System.Threading.Tasks;
using AutoMapper;
using Convey.CQRS.Queries;
using IconService.Models.Dto;
using IconService.Services;

namespace IconService.Messages.Queries
{
    public class GetIconQuery : IQuery<IconDto>
    {
        public string Icon { get; set; }
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
            IconDto iconDto = null;

            var iconDocument =
                await _iconService.GetAsync(query.Icon);

            if (iconDocument != null)
            {
                iconDto = _mapper.Map<IconDto>(iconDocument);
            }

            return iconDto;
        }
    }
}