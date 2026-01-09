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
public class CitiesService : Cities.CitiesBase
{
    private readonly IGenericRepository<CityInfo> cityInfoRepo;
    private readonly IMapper mapper;
    private readonly IMediator mediator;

    public CitiesService(
        IMapper mapper,
        IGenericRepository<CityInfo> cityInfoRepo,
        IMediator mediator)
    {
        this.mapper = mapper;
        this.cityInfoRepo = cityInfoRepo;
        this.mediator = mediator;
    }

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
            NumberOfAllCities = result.NumberOfAllCities,
        };
        citiesPaginationReply.Cities.AddRange(mapper.Map<RepeatedField<CityReply>>(result.Cities));

        return citiesPaginationReply;
    }

    public override async Task GetCitiesStream(
        CitiesStreamRequest request,
        IServerStreamWriter<CityReply> responseStream,
        ServerCallContext context)
    {
        var query = new GetCitiesPaginationQuery { NumberOfCities = request.NumberOfCities, PageNumber = request.PageNumber };
        var result = await mediator.SendAsync(query);

        var cities = mapper.Map<List<CityReply>>(result.Cities);

        foreach (var city in cities)
        {
            await responseStream.WriteAsync(city);
        }
    }
}