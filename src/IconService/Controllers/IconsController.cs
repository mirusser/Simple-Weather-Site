﻿using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Convey.CQRS.Queries;
using IconService.Messages.Queries;
using IconService.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace IconService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class IconsController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;

        public IconsController(
            IQueryDispatcher queryDispatcher,
            ICommandDispatcher commandDispatcher)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
        }

        [HttpGet("{Icon}", Name = "GetByCityName")]
        public async Task<IconDto?> GetIcon([FromRoute] GetIconQuery query)
        {
            return await _queryDispatcher.QueryAsync(query);
        }
    }
}