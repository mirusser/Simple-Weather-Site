using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WeatherSite.Clients.Models.Records;
using WeatherSite.Settings;

namespace WeatherSite.Logic.Clients;

public class IconClient(
    HttpClient httpClient,
    IOptions<ApiEndpoints> options,
    JsonSerializerOptions jsonSerializerOptions)
{
    private readonly ApiEndpoints _apiEndpoints = options.Value;

    public async Task<IconDto?> GetIcon(string icon)
    {
        var url = $"{_apiEndpoints.IconServiceApiUrl}Get";
        var query = new { icon };
        var response = await httpClient.PostAsJsonAsync(url, query);
        var responseJson = await response.Content.ReadAsStringAsync();
        var iconDto = JsonSerializer.Deserialize<IconDto>(
            responseJson,
            jsonSerializerOptions);

        return iconDto;
    }
}