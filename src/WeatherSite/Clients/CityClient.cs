using Microsoft.Extensions.Options;
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
            string url = $"{_apiEndpoints.CitiesServiceApiUrl}{HttpUtility.UrlEncode(cityName)}/{HttpUtility.UrlEncode(limit.ToString())}";
            List<City>  cities = await _httpClient.GetFromJsonAsync<List<City>>(url);

            return cities;
        }

        public async Task<CitiesPagination> GetCitiesPagination(int pageNumber = 1, int numberOfCities = 25)
        {
            string url = $"{_apiEndpoints.CitiesServiceApiUrl}GetCitiesPagination/{HttpUtility.UrlEncode(numberOfCities.ToString())}/{HttpUtility.UrlEncode(pageNumber.ToString())}";
            CitiesPagination citiesPagination = await _httpClient.GetFromJsonAsync<CitiesPagination>(url);

            return citiesPagination;
        }
    }
}
