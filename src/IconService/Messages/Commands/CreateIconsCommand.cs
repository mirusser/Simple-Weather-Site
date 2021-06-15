using AutoMapper;
using Convey.CQRS.Commands;
using IconService.Models.Dto;
using IconService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IconService.Messages.Commands
{
    public class CreateIconsCommand : ICommand
    {
        public List<CreateIconDto> Icons { get; set; }
    }

    public class CreateIconsHandler : ICommandHandler<CreateIconsCommand>
    {
        private readonly IIconService _iconService;
        private readonly IMapper _mapper;

        public async Task HandleAsync(CreateIconsCommand command)
        {
            // await _cityManager.SaveCitiesFromFileToDatabase();
        }
    }
}
