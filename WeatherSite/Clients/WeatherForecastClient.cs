﻿using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WeatherSite.Settings;

namespace WeatherSite.Clients
{
    public class WeatherForecastClient
    {
        #region Properties
        private readonly HttpClient _httpClient;
        private readonly ApiEndpoints _apiEndpoints;
        #endregion

        #region records
        public record WeatherForecast(DateTime Date, int TemperatureC, int TemperatureF, string Summary);
        #endregion
        
        public WeatherForecastClient(
            HttpClient httpClient,
            IOptions<ApiEndpoints> options)
        {
            _httpClient = httpClient;
            _apiEndpoints = options.Value;
        }

        public async Task<WeatherForecast> GetCurrentWeatherForCity(string city = "Poznan")
        {
            WeatherForecast weatherForecast = null;
            try
            {
                weatherForecast = await _httpClient.GetFromJsonAsync<WeatherForecast>($"{_apiEndpoints.WeatherServiceApiUrl}{city}");
            }
            catch (Exception ex)
            {
                //TODO: do something with error here
            }

            return weatherForecast;
        }
    }
}
