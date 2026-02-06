using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CitiesGrpcService;
using Common.Infrastructure.Consts;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Presentation.Http;
using GrpcCitiesClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherSite.Logic.Clients.Models.Records;
using WeatherSite.Logic.Settings;
using WeatherSite.Models;

namespace WeatherSite.Logic.Clients;

public class CityManager(
    IHttpExecutor httpExecutor,
    IHttpRequestFactory requestFactory,
    ICitiesClient citiesClient, // grpc client
    ILogger<CityManager> logger,
    IOptions<ApiEndpoints> options)
{
    private readonly ApiEndpoints apiEndpoints = options.Value;

    public async Task<List<City>> GetCitiesByName(
        string cityName,
        int limit = 10,
        CancellationToken ct = default)
    {
        string url = $"{apiEndpoints.CitiesServiceApiUrl}GetCitiesByName";

        using var request = requestFactory.Create(
            url,
            new
            {
                cityName,
                limit
            },
            HttpMethod.Post.Method);

        using var res = await httpExecutor.SendAsync(
            HttpClientConsts.DefaultName,
            PipelineNames.Default,
            request,
            ct);

        var result = await HttpResult.ReadJsonAsResultAsync<CitiesResponse>(res, ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Could not get cities by name {cityName}. Problem: {Problem}", cityName, result.Problem);

            return [];
        }

        return result.Value.Cities;
    }

    public async Task<PaginationVM<CityReply>> GetCitiesPaginationPartialVMAsync(
        string url,
        int pageNumber = 1,
        int numberOfEntitiesOnPage = 25)
    {
        //using gRPC (unary)
        var citiesPaginationReply = await citiesClient.GetCitiesPagination(pageNumber, numberOfEntitiesOnPage);

        //using gRPC (stream from server)
        //var cities = new List<CityReply>();
        //await foreach (var cityReply in _citiesClient.GetCitiesStream(pageNumber, numberOfEntitiesOnPage))
        //{
        //    cities.Add(cityReply);
        //}

        var numberOfPages = Convert.ToInt32(
            Math.Ceiling((decimal)citiesPaginationReply?.NumberOfAllCities / numberOfEntitiesOnPage));

        PaginationVM<CityReply> vm = new()
        {
            IsSuccess = citiesPaginationReply?.Cities is not null,
            Values = citiesPaginationReply?.Cities?.ToList(),
            ElementId = "#cities-pagination-partial-div",
            Url = url,
            PageNumber = pageNumber,
            NumberOfEntitiesOnPage = numberOfEntitiesOnPage,
            NumberOfPages = numberOfPages
        };

        return vm;
    }
}