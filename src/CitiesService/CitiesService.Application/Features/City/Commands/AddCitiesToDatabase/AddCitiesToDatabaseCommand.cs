using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Services;
using CitiesService.Domain.Entities;
using Common.Mediator;
using Common.Presentation.Http;

namespace CitiesService.Application.Features.City.Commands.AddCitiesToDatabase;

public class AddCitiesToDatabaseCommand : IRequest<Result<AddCitiesToDatabaseResult>>;

public class AddCitiesToDatabaseHandler(
    IGenericRepository<CityInfo> cityInfoRepo,
    ICitiesSeeder citiesSeeder)
    : IRequestHandler<AddCitiesToDatabaseCommand, Result<AddCitiesToDatabaseResult>>
{
    public async Task<Result<AddCitiesToDatabaseResult>> Handle(
        AddCitiesToDatabaseCommand request,
        CancellationToken cancellationToken)
    {
        var anyCityExists = await cityInfoRepo
            .CheckIfExistsAsync(c => c.Id != 0, cancellationToken);

        if (anyCityExists)
        {
            return Result<AddCitiesToDatabaseResult>.Fail(Problems.Conflict("Cities already added"));
        }

        var isSuccess = await citiesSeeder.SeedIfEmptyAsync(cancellationToken);

        return Result<AddCitiesToDatabaseResult>.Ok(new AddCitiesToDatabaseResult
            { IsSuccess = isSuccess, IsAlreadyAdded = anyCityExists });
    }
}