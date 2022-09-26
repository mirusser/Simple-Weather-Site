using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CitiesService.Application.Interfaces.Managers;
using Google.Protobuf.Collections;
using Grpc.Core;

namespace CitiesGrpcService.Services;

public class CitiesService : Cities.CitiesBase
{
    private readonly ICityManager _cityManager;
    private readonly IMapper _mapper;

    public CitiesService(
        ICityManager cityManager,
        IMapper mapper)
    {
        _cityManager = cityManager;
        _mapper = mapper;
    }

    public override async Task<CitiesPaginationInfoReply> GetCitiesPaginationInfo(CitiesPaginationInfoRequest request, ServerCallContext context)
    {
        var countOfAllCities = await _cityManager.GetCountOfAllCities();
        var citiesPaginationInfoReply = new CitiesPaginationInfoReply() { NumberOfAllCities = countOfAllCities };

        return citiesPaginationInfoReply;
    }

    public override async Task<CitiesPaginationReply> GetCitiesPagination(CitiesPaginationRequest request, ServerCallContext context)
    {
        var getCitiesInfoPagination = await _cityManager.GetCitiesInfoPagination(request.NumberOfCities, request.PageNumber);

        var citiesPaginationReply = _mapper.Map<CitiesPaginationReply>(getCitiesInfoPagination);
        citiesPaginationReply.Cities.AddRange(_mapper.Map<RepeatedField<CityReply>>(getCitiesInfoPagination.CityInfos.ToList()));

        return citiesPaginationReply;
    }

    public override async Task GetCitiesStream(
        CitiesStreamRequest request,
        IServerStreamWriter<CityReply> responseStream,
        ServerCallContext context)
    {
        var getCitiesInfoPagination = await _cityManager.GetCitiesInfoPagination(request.NumberOfCities, request.PageNumber);
        var cities = _mapper.Map<List<CityReply>>(getCitiesInfoPagination.CityInfos.ToList());

        foreach (var city in cities)
        {
            await responseStream.WriteAsync(city);
        }
    }
}