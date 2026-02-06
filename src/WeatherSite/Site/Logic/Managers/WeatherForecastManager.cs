using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Infrastructure.Consts;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Presentation.Http;
using Microsoft.Extensions.Options;
using WeatherSite.Logic.Managers.Models.Records;
using WeatherSite.Logic.Settings;
using WeatherSite.Models.WeatherPrediction;

namespace WeatherSite.Logic.Managers;

public class WeatherForecastManager(
    IHttpExecutor httpExecutor,
    IHttpRequestFactory  requestFactory,
    IOptions<ApiEndpoints> options)
{
    private readonly ApiEndpoints apiEndpoints = options.Value;

    public GetWeatherForecastVM GetGetWeatherForecastVm()
    {
        var vm = new GetWeatherForecastVM()
        {
            CitiesServiceEndpoint = apiEndpoints.CitiesServiceApiUrl
        };

        return vm;
    }

    // TODO: magic strings
    public async Task<Result<WeatherForecast>> GetCurrentWeatherForCityByCityIdAsync(
        decimal cityId,
        CancellationToken ct)
    {
        var url = $"{apiEndpoints.WeatherServiceApiUrl}GetByCityId";
        
        using var request = requestFactory.Create(
            url,
            new { CityId = cityId },
            HttpMethod.Post.Method);
        
        using var res = await httpExecutor.SendAsync(
            HttpClientConsts.DefaultName, 
            PipelineNames.Default,
            request, 
            ct);

        return await HttpResult.ReadJsonAsResultAsync<WeatherForecast>(res, ct);
    }
}