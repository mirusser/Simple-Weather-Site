using Microsoft.Extensions.Logging;
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
    public class WeatherHistoryClient
    {
        #region Properties
        private readonly HttpClient _httpClient;
        private readonly ApiEndpoints _apiEndpoints;
        private readonly ILogger<WeatherHistoryClient> _logger;
        public string Url { get; set; }
        #endregion

        public WeatherHistoryClient(
            HttpClient httpClient,
            IOptions<ApiEndpoints> options,
            ILogger<WeatherHistoryClient> logger)
        {
            _httpClient = httpClient;
            _apiEndpoints = options.Value;
            _logger = logger;
        }

        public async Task<WeatherHistoryForecastPagination> GetWeatherHistoryForecastPagination(int pageNumber = 1, int numberOfEntities = 25)
        {
            string url = $"{_apiEndpoints.WeatherHistoryServiceApiUrl}{HttpUtility.UrlEncode(numberOfEntities.ToString())}/{HttpUtility.UrlEncode(pageNumber.ToString())}";
            var citiesPagination = await _httpClient.GetFromJsonAsync<WeatherHistoryForecastPagination>(url);

            return citiesPagination;
        }
    }
}
