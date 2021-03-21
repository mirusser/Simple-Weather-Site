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
            WeatherForecast weatherForecast = null;
            try
            {
                var url = $"{_apiEndpoints.WeatherServiceApiUrl}GetByCityName/{city}";
                weatherForecast = await _httpClient.GetFromJsonAsync<WeatherForecast>(url);
            }
            catch (Exception ex)
            {
                //TODO: do something with error here
            }

            return weatherForecast;
        }

        public async Task<WeatherForecast> GetCurrentWeatherForCityByCityId(decimal cityId)
        {
            WeatherForecast weatherForecast = null;
            try
            {
                var url = $"{_apiEndpoints.WeatherServiceApiUrl}GetByCityId/{cityId}";
                weatherForecast = await _httpClient.GetFromJsonAsync<WeatherForecast>(url);
            }
            catch (Exception ex)
            {
                //TODO: do something with error here
            }

            return weatherForecast;
        }
    }
}
