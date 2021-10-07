using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Convey.CQRS.Queries;
using IconService.Models.Dto;
using IconService.Services;
using IconService.Settings;
using Microsoft.Extensions.Options;

namespace IconService.Messages.Queries
{
    public class GetIconQuery : IQuery<IconDto>
    {
        public string Icon { get; set; }
    }

    public class GetIconQueryHandler : IQueryHandler<GetIconQuery, IconDto>
    {
        private readonly ServiceSettings _serviceSettings;
        private readonly IIconService _iconService;
        private readonly IMapper _mapper;

        public GetIconQueryHandler(
            IOptions<ServiceSettings> options,
            IIconService iconService,
            IMapper mapper)
        {
            _serviceSettings = options.Value;
            _iconService = iconService;
            _mapper = mapper;
        }

        public async Task<IconDto> HandleAsync(GetIconQuery query)
        {
            IconDto iconDto = null;

            try
            {
                var iconDocument =
                    await _iconService.GetAsync(query.Icon);

                if (iconDocument != null)
                {
                    iconDto = _mapper.Map<IconDto>(iconDocument);
                    var iconPath = $"{_serviceSettings.IconsPath}\\{iconDto.Name}";

                    if (File.Exists(iconPath))
                    {
                        iconDto.FileContent = File.ReadAllBytes(iconPath);
                    }
                }
            }
            catch (Exception ex)
            {
                var x = ex;
            }

            return iconDto;
        }
    }
}