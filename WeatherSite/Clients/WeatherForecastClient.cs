using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WeatherSite.Clients.Models.Records;
using WeatherSite.Settings;

namespace WeatherSite.Clients
{
    public class WeatherForecastClient
    {
        #region Properties
        private readonly HttpClient _httpClient;
        private readonly ApiEndpoints _apiEndpoints;
        #endregion

        public WeatherForecastClient(
            HttpClient httpClient,
            IOptions<ApiEndpoints> options)
        {
            _httpClient = httpClient;
            _apiEndpoints = options.Value;
        }

        public async Task<WeatherForecast> GetCurrentWeatherForCityByCityName(string city)
        {
            string url = $"{_apiEndpoints.WeatherServiceApiUrl}GetByCityName/{city}";
            WeatherForecast weatherForecast = await _httpClient.GetFromJsonAsync<WeatherForecast>(url);

            return weatherForecast;
        }

        public async Task<WeatherForecast> GetCurrentWeatherForCityByCityId(decimal cityId)
        {
            string url = $"{_apiEndpoints.WeatherServiceApiUrl}GetByCityId/{cityId}";
            WeatherForecast weatherForecast = await _httpClient.GetFromJsonAsync<WeatherForecast>(url);

            return weatherForecast;
        }
    }
}
