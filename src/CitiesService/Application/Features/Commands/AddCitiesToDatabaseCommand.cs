using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Interfaces.Managers;
using CitiesService.Domain.Models;
using MediatR;

namespace CitiesService.Application.Features.Commands;

public class AddCitiesToDatabaseCommand : IRequest<Response<bool>>
{
}

public class AddCitiesToDatabaseHandler : IRequestHandler<AddCitiesToDatabaseCommand, Response<bool>>
{
    private readonly ICityManager _cityManager;

    public AddCitiesToDatabaseHandler(ICityManager cityManager)
    {
        _cityManager = cityManager;
    }

    public async Task<Response<bool>> Handle(AddCitiesToDatabaseCommand request, CancellationToken cancellationToken)
    {
        var response = new Response<bool>
        {
            IsSuccess = await _cityManager.SaveCitiesFromFileToDatabase()
        };

        return response;
    }
}