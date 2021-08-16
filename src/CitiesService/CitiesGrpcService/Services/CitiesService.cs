using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using CitiesGrpcService;
using Application.Interfaces.Managers;
using AutoMapper;

namespace CitiesGrpcService
{
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

        public override Task<CitiesPaginationInfo> GetCitiesPaginationInfo(CitiesPaginationInfoRequest request, ServerCallContext context)
        {
            return base.GetCitiesPaginationInfo(request, context);
        }

        public override Task<CitiesPaginationReply> GetCitiesPagination(CitiesPaginationRequest request, ServerCallContext context)
        {
            return base.GetCitiesPagination(request, context);
        }

        //TODO: add automapper
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
}
