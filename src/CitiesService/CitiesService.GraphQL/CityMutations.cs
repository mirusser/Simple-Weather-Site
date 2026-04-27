using CitiesService.Application.Features.City.Commands.UpdateCity;
using CitiesService.GraphQL.Types;
using Common.Mediator;
using Common.Presentation.Http;

namespace CitiesService.GraphQL;

public class CityMutations
{
    public async Task<UpdateCityPayload> UpdateCityAsync(
        UpdateCityInput input,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        using var activity = GraphQlTelemetry.StartActivity("GraphQL.UpdateCity");

        try
        {
            var cmd = new UpdateCityCommand
            {
                Id = input.Id,
                CityId = input.CityId,
                Name = input.Name,
                State = input.State,
                CountryCode = input.CountryCode,
                Lon = input.Lon,
                Lat = input.Lat,
                RowVersion = input.RowVersion
            };

            var result = await mediator.SendAsync(cmd, ct);

            if (!result.IsSuccess)
            {
                GraphQlTelemetry.SetResult(activity, "failure");
                throw ToGraphQlException(result.Problem!);
            }

            GraphQlTelemetry.SetResult(activity, "success");
            return new UpdateCityPayload(result.Value!.City);
            //return new UpdateCityPayload(result);
        }
        catch (Exception ex)
        {
            GraphQlTelemetry.SetException(activity, ex);
            throw;
        }
    }

    public async Task<UpdateCityPayload> PatchCityAsync(
        PatchCityInput input,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        using var activity = GraphQlTelemetry.StartActivity("GraphQL.PatchCity");

        try
        {
            var cmd = new PatchCityCommand
            {
                Id = input.Id,
                CityId = input.CityId,
                Name = input.Name,
                State = input.State,
                CountryCode = input.CountryCode,
                Lon = input.Lon,
                Lat = input.Lat,
                RowVersion = input.RowVersion
            };

            var result = await mediator.SendAsync(cmd, ct);

            if (!result.IsSuccess)
            {
                GraphQlTelemetry.SetResult(activity, "failure");
                throw ToGraphQlException(result.Problem!);
            }

            GraphQlTelemetry.SetResult(activity, "success");
            return new UpdateCityPayload(result.Value!.City);
            //return new UpdateCityPayload(result);
        }
        catch (Exception ex)
        {
            GraphQlTelemetry.SetException(activity, ex);
            throw;
        }
    }

    private static GraphQLException ToGraphQlException(Problem p) =>
        new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(p.Message)
                .SetCode(p.Code)
                .SetExtension("status", p.Status)
                .Build());
}
