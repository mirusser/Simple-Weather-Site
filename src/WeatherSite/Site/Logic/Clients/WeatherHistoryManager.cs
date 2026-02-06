using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Infrastructure.Consts;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Presentation.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherSite.Logic.Clients.Models.Records;
using WeatherSite.Logic.Settings;
using WeatherSite.Models;
using WeatherSite.Models.WeatherHistory;

namespace WeatherSite.Logic.Clients;

public class WeatherHistoryManager(
    IHttpExecutor httpExecutor,
    IHttpRequestFactory requestFactory,
    IOptions<ApiEndpoints> options,
    ILogger<WeatherHistoryManager> logger)
{
    private readonly ApiEndpoints apiEndpoints = options.Value;

    [HttpGet]
    public WeatherHistoryVM GetWeatherHistoryVm()
    {
        WeatherHistoryVM vm = new()
        {
            SignalRServerUrl = apiEndpoints.SignalRServer
        };

        return vm;
    }
    
    public async Task<PaginationVM<CityWeatherForecastDocument>> GetWeatherHistoryPaginationVmAsync(
        string url,
        int pageNumber = 1,
        int numberOfEntitiesOnPage = 25,
        CancellationToken  ct = default)
    {
        var result =
            await GetWeatherHistoryForecastPaginationAsync(
                pageNumber, 
                numberOfEntitiesOnPage,
                ct);

        PaginationVM<CityWeatherForecastDocument> vm = new()
        {
            IsSuccess = result.IsSuccess,
            ElementId = "#weather-history-pagination-partial-div",
            Url = url,
            PageNumber = pageNumber,
            NumberOfEntitiesOnPage = numberOfEntitiesOnPage
        };

        if (result.IsSuccess)
        {
            vm.Values = result.Value.WeatherForecastDocuments;
            vm.NumberOfPages =
                Convert.ToInt32(
                    Math.Ceiling((decimal)result.Value.NumberOfAllEntities / numberOfEntitiesOnPage));
        }

        return vm;
    }
    
    // TODO: get rid of magic string
    public async Task<Result<WeatherHistoryForecastPagination>> GetWeatherHistoryForecastPaginationAsync(
        int pageNumber = 1,
        int numberOfEntities = 25,
        CancellationToken  ct = default)
    {
        var url = $"{options.Value.WeatherHistoryServiceApiUrl}GetCityWeatherForecastPagination";
        
        using var request = requestFactory.Create(
            url,
            new
            {
                numberOfEntities,
                pageNumber
            },
            HttpMethod.Post.Method);

        using var res = await httpExecutor.SendAsync(
            HttpClientConsts.DefaultName,
            PipelineNames.Default,
            request,
            ct);

        var result = await HttpResult.ReadJsonAsResultAsync<WeatherHistoryForecastPagination>(res, ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Could not get weather history forecasts");
        }

        return result;
    }
}