using CitiesService.Logic.Managers.Contracts;
using Convey.CQRS.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesService.Messages.Commands.Handlers
{
    public class AddCitiesToDatabaseHandler : ICommandHandler<AddCitiesToDatabaseOrder>
    {
        private readonly ICityManager _cityManager;

        public AddCitiesToDatabaseHandler(ICityManager cityManager)
        {
            _cityManager = cityManager;
        }

        public async Task HandleAsync(AddCitiesToDatabaseOrder command)
        {
            await _cityManager.SaveCitiesFromFileToDatabase();
        }
    }
}
