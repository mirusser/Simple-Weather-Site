﻿using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using WeatherSite.Clients.Models.Records;
using WeatherSite.Settings;

namespace WeatherSite.Clients
{
    public class CityClient
    {
        #region Properties
        private readonly HttpClient _httpClient;
        private readonly ApiEndpoints _apiEndpoints;
        #endregion

        public CityClient(HttpClient httpClient,
                IOptions<ApiEndpoints> options)
        {
            _httpClient = httpClient;
            _apiEndpoints = options.Value;
        }

        public async Task<List<City>> GetCitiesByName(string cityName, int limit = 10)
        {
            List<City> cities = new();

            try
            {
                var url = $"{_apiEndpoints.CitiesServiceApiUrl}{HttpUtility.UrlEncode(cityName)}/{HttpUtility.UrlEncode(limit.ToString())}";
                cities = await _httpClient.GetFromJsonAsync<List<City>>(url);
            }
            catch (Exception ex)
            {
                //TODO: do something with error here
            }

            return cities;
        }
    }
}
