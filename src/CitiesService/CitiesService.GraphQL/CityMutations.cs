using CitiesService.Application.Features.City.Commands.AddCitiesToDatabase;
using CitiesService.Application.Features.City.Models.Dto;
using Common.Mediator;

namespace CitiesService.GraphQL;

public class CityMutations
{
    // TODO: add real mutation here:
    public async Task<AddCitiesToDatabasePayload> AddCitiesToDatabaseAsync(
        AddCitiesToDatabaseInput input,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var command = new AddCitiesToDatabaseCommand
        {
            // map fields from input
            // ...
        };

        var result =
            await mediator.SendAsync(command, ct);

        if (!result.IsSuccess)
        {;
            // Raise a GraphQL error with nice extensions
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(result.Problem?.Message)
                    .SetCode(result.Problem?.Code)
                    .SetExtension("status", result.Problem?.Status)
                    .Build());
        }

        var v = result.Value;
        return new AddCitiesToDatabasePayload(v.IsSuccess, v.IsAlreadyAdded);
    }
}

public record AddCitiesToDatabaseInput(bool Run);

public record AddCitiesToDatabasePayload(bool IsSuccess, bool IsAlreadyAdded);