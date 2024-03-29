﻿using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WeatherSite.Clients.Models.Records;
using WeatherSite.Settings;

namespace WeatherSite.Clients;

public class WeatherForecastClient
{
    #region Properties

    private readonly HttpClient _httpClient;
    private readonly ApiEndpoints _apiEndpoints;

    #endregion Properties

    public WeatherForecastClient(
        HttpClient httpClient,
        IOptions<ApiEndpoints> options)
    {
        _httpClient = httpClient;
        _apiEndpoints = options.Value;
    }

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

        var response = await _httpClient.PostAsJsonAsync(url, query);
        var responseJson = await response.Content.ReadAsStringAsync();
        var weatherForecast = JsonSerializer.Deserialize<WeatherForecast>(responseJson, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

        //WeatherForecast weatherForecast = await _httpClient.GetFromJsonAsync<WeatherForecast>(url);

        return weatherForecast;
    }
}