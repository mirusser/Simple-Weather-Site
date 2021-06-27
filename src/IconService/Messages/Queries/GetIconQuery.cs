using AutoMapper;
using Convey.CQRS.Queries;
using IconService.Models.Dto;
using IconService.Mongo.Documents;
using IconService.Services;
using IconService.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
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

            var iconDocument =
                await _iconService.GetAsync(query.Description, query.DayIcon);

            if (iconDocument != null)
            {
                iconDto = _mapper.Map<IconDto>(iconDocument);
                var iconPath = $"{_serviceSettings.IconsPath}\\{iconDto.Name}";

                if (File.Exists(iconPath))
                {
                    iconDto.FileContent = File.ReadAllBytes(iconPath);
                }
            }

            return iconDto;
        }
    }
}
