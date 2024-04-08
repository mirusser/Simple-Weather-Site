using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WeatherSite.Clients.Models.Records;
using WeatherSite.Settings;

namespace WeatherSite.Clients;

public class WeatherHistoryClient(
    HttpClient httpClient,
    IOptions<ApiEndpoints> options,
    JsonSerializerOptions jsonSerializerOptions)
{
    private readonly string url = options.Value.WeatherHistoryServiceApiUrl;

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

        HttpResponseMessage response = await httpClient.PostAsync($"{url}GetCityWeatherForecastPagination", jsonContent);
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