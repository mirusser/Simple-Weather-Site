using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CitiesGrpcService.Telemetry;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Application.Features.City.Queries.GetCitiesPagination;
using CitiesService.Application.Telemetry;
using CitiesService.Domain.Entities;
using Common.Mediator;
using Google.Protobuf.Collections;
using Grpc.Core;
using MapsterMapper;

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
        const string method = CitiesTelemetryConventions.Operations.Grpc.GetCitiesPaginationInfo;
        const string grpcType = CitiesTelemetryConventions.GrpcTypes.Unary;
        using var activity = CitiesGrpcTelemetry.StartActivity(method, grpcType);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            var countOfAllCities = await cityInfoRepo.CountAsync(_ => true, context.CancellationToken);
            var citiesPaginationInfoReply = new CitiesPaginationInfoReply() { NumberOfAllCities = countOfAllCities, };

            CitiesGrpcTelemetry.SetResult(activity, CitiesTelemetryConventions.ResultValues.Success);
            CitiesGrpcTelemetry.RecordCall(
                method,
                grpcType,
                Stopwatch.GetElapsedTime(startedAt),
                CitiesTelemetryConventions.ResultValues.Success);

            return citiesPaginationInfoReply;
        }
        catch (Exception ex)
        {
            CitiesGrpcTelemetry.SetException(activity, ex);
            CitiesGrpcTelemetry.RecordCall(
                method,
                grpcType,
                Stopwatch.GetElapsedTime(startedAt),
                CitiesTelemetryConventions.ResultValues.Exception,
                ex.GetType().Name);

            throw;
        }
    }

    public override async Task<CitiesPaginationReply> GetCitiesPagination(
        CitiesPaginationRequest request,
        ServerCallContext context)
    {
        const string method = CitiesTelemetryConventions.Operations.Grpc.GetCitiesPagination;
        const string grpcType = CitiesTelemetryConventions.GrpcTypes.Unary;
        using var activity = CitiesGrpcTelemetry.StartActivity(method, grpcType);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            var query = new GetCitiesPaginationQuery
            {
                NumberOfCities = request.NumberOfCities,
                PageNumber = request.PageNumber
            };
            var result = await mediator.SendAsync(query, context.CancellationToken);

            var citiesPaginationReply = new CitiesPaginationReply()
            {
                NumberOfAllCities = result.Value.NumberOfAllCities,
            };
            citiesPaginationReply.Cities.AddRange(mapper.Map<RepeatedField<CityReply>>(result.Value.Cities));

            CitiesGrpcTelemetry.SetResult(activity, CitiesTelemetryConventions.ResultValues.Success);
            CitiesGrpcTelemetry.RecordCall(
                method,
                grpcType,
                Stopwatch.GetElapsedTime(startedAt),
                CitiesTelemetryConventions.ResultValues.Success);

            return citiesPaginationReply;
        }
        catch (Exception ex)
        {
            CitiesGrpcTelemetry.SetException(activity, ex);
            CitiesGrpcTelemetry.RecordCall(
                method,
                grpcType,
                Stopwatch.GetElapsedTime(startedAt),
                CitiesTelemetryConventions.ResultValues.Exception,
                ex.GetType().Name);

            throw;
        }
    }

    public override async Task GetCitiesStream(
        CitiesStreamRequest request,
        IServerStreamWriter<CityReply> responseStream,
        ServerCallContext context)
    {
        const string method = CitiesTelemetryConventions.Operations.Grpc.GetCitiesStream;
        const string grpcType = CitiesTelemetryConventions.GrpcTypes.ServerStreaming;
        using var activity = CitiesGrpcTelemetry.StartActivity(method, grpcType);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            var query = new GetCitiesPaginationQuery
            {
                NumberOfCities = request.NumberOfCities,
                PageNumber = request.PageNumber
            };
            var result = await mediator.SendAsync(query, context.CancellationToken);

            var cities = mapper.Map<List<CityReply>>(result.Value.Cities);

            foreach (var city in cities)
            {
                await responseStream.WriteAsync(city);
                CitiesGrpcTelemetry.RecordStreamMessage(method);
            }

            CitiesGrpcTelemetry.SetResult(activity, CitiesTelemetryConventions.ResultValues.Success);
            CitiesGrpcTelemetry.RecordCall(
                method,
                grpcType,
                Stopwatch.GetElapsedTime(startedAt),
                CitiesTelemetryConventions.ResultValues.Success);
        }
        catch (Exception ex)
        {
            CitiesGrpcTelemetry.SetException(activity, ex);
            CitiesGrpcTelemetry.RecordCall(
                method,
                grpcType,
                Stopwatch.GetElapsedTime(startedAt),
                CitiesTelemetryConventions.ResultValues.Exception,
                ex.GetType().Name);

            throw;
        }
    }
}
