using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WeatherSite.Clients.Models.Records;
using WeatherSite.Settings;

namespace WeatherSite.Clients;

public class WeatherForecastClient(
    HttpClient httpClient,
    IOptions<ApiEndpoints> options,
    JsonSerializerOptions jsonSerializerOptions)
{

    #region Properties
    private readonly ApiEndpoints _apiEndpoints = options.Value;

    #endregion Properties

    //public async Task<WeatherForecast> GetCurrentWeatherForCityByCityName(string city)
    //{
    //    string url = $"{_apiEndpoints.WeatherServiceApiUrl}GetByCityName/{city}";
    //    WeatherForecast weatherForecast = await _httpClient.GetFromJsonAsync<WeatherForecast>(url);

    //    return weatherForecast;
    //}

    public async Task<WeatherForecast> GetCurrentWeatherForCityByCityId(decimal cityId)
    {
        var query = new { CityId = cityId };
        string url = $"{_apiEndpoints.WeatherServiceApiUrl}GetByCityId";

        var response = await httpClient.PostAsJsonAsync(url, query);
        var responseJson = await response.Content.ReadAsStringAsync();
        var weatherForecast = JsonSerializer.Deserialize<WeatherForecast>(responseJson, jsonSerializerOptions);

        //WeatherForecast weatherForecast = await _httpClient.GetFromJsonAsync<WeatherForecast>(url);

        return weatherForecast;
    }
}