using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WeatherSite.Clients.Models.Records;
using WeatherSite.Settings;

namespace WeatherSite.Logic.Clients;

public class IconClient
{
    #region Properties

    private readonly HttpClient _httpClient;
    private readonly ApiEndpoints _apiEndpoints;

    #endregion Properties

    public IconClient(
        HttpClient httpClient,
        IOptions<ApiEndpoints> options)
    {
        _httpClient = httpClient;
        _apiEndpoints = options.Value;
    }

    public async Task<IconDto?> GetIcon(string icon)
    {
        var url = $"{_apiEndpoints.IconServiceApiUrl}Get";
        var query = new { icon };
        var response = await _httpClient.PostAsJsonAsync(url, query);
        var responseJson = await response.Content.ReadAsStringAsync();
        var iconDto = JsonSerializer.Deserialize<IconDto>(
            responseJson,
            new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

        return iconDto;
    }
}