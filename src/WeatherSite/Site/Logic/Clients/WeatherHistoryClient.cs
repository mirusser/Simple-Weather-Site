using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherSite.Clients.Models.Records;
using WeatherSite.Settings;

namespace WeatherSite.Logic.Clients;

public class WeatherHistoryClient(
    HttpClient httpClient,
    IOptions<ApiEndpoints> options,
    JsonSerializerOptions jsonSerializerOptions,
    ILogger<WeatherHistoryClient> logger)
{
    // TODO: get rid of magic string
    public async Task<WeatherHistoryForecastPagination> GetWeatherHistoryForecastPagination(
        int pageNumber = 1,
        int numberOfEntities = 25)
    {
        var getCityWeatherForecastPaginationQuery = new
        {
            numberOfEntities,
            pageNumber
        };

        var jsonContent = JsonContent.Create(getCityWeatherForecastPaginationQuery);

        var url = $"{options.Value.WeatherHistoryServiceApiUrl}GetCityWeatherForecastPagination";
        logger.LogDebug("Getting weather history forecast pagination using {url}", url);

        HttpResponseMessage response = await httpClient.PostAsync(url, jsonContent);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var citiesPagination =
            JsonSerializer
                .Deserialize<WeatherHistoryForecastPagination>(
                    content,
                    jsonSerializerOptions);

        return citiesPagination ?? new([], default);
    }
}