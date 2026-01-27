using System.Collections.Generic;
using System.Threading.Tasks;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Queries.GetCitiesPagination;
using CitiesService.Domain.Entities;
using Common.Mediator;
using Google.Protobuf.Collections;
using Grpc.Core;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace CitiesGrpcService.Services;

//TODO: mapping (check)
public class CitiesService(
    IMapper mapper,
    IGenericRepository<CityInfo> cityInfoRepo,
    IMediator mediator)
    : Cities.CitiesBase
{
    public override async Task<CitiesPaginationInfoReply> GetCitiesPaginationInfo(
        CitiesPaginationInfoRequest request,
        ServerCallContext context)
    {
        var countOfAllCities = await cityInfoRepo.FindAll(searchExpression: _ => true).CountAsync();
        var citiesPaginationInfoReply = new CitiesPaginationInfoReply() { NumberOfAllCities = countOfAllCities, };

        return citiesPaginationInfoReply;
    }

    public override async Task<CitiesPaginationReply> GetCitiesPagination(
        CitiesPaginationRequest request,
        ServerCallContext context)
    {
        var query = new GetCitiesPaginationQuery { NumberOfCities = request.NumberOfCities, PageNumber = request.PageNumber };
        var result = await mediator.SendAsync(query);

        var citiesPaginationReply = new CitiesPaginationReply()
        {
            NumberOfAllCities = result.Value.NumberOfAllCities,
        };
        citiesPaginationReply.Cities.AddRange(mapper.Map<RepeatedField<CityReply>>(result.Value.Cities));

        return citiesPaginationReply;
    }

    public override async Task GetCitiesStream(
        CitiesStreamRequest request,
        IServerStreamWriter<CityReply> responseStream,
        ServerCallContext context)
    {
        var query = new GetCitiesPaginationQuery { NumberOfCities = request.NumberOfCities, PageNumber = request.PageNumber };
        var result = await mediator.SendAsync(query);

        var cities = mapper.Map<List<CityReply>>(result.Value.Cities);

        foreach (var city in cities)
        {
            await responseStream.WriteAsync(city);
        }
    }
}