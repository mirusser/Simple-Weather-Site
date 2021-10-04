using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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

        #endregion Properties

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
            //string url = $"{_apiEndpoints.WeatherHistoryServiceApiUrl}{HttpUtility.UrlEncode(numberOfEntities.ToString())}/{HttpUtility.UrlEncode(pageNumber.ToString())}";
            //var citiesPagination = await _httpClient.GetFromJsonAsync<WeatherHistoryForecastPagination>(url);

            string url = $"{_apiEndpoints.WeatherHistoryServiceApiUrl}GetCityWeatherForecastPagination";

            var getCityWeatherForecastPaginationQuery = new
            {
                numberOfEntities,
                pageNumber
            };

            var jsonContent = JsonContent.Create(getCityWeatherForecastPaginationQuery);

            HttpResponseMessage response = await _httpClient.PostAsync(url, jsonContent);

            var content = await response.Content.ReadAsStringAsync();
            var citiesPagination =
                JsonSerializer
                .Deserialize<WeatherHistoryForecastPagination>(
                    content,
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            return citiesPagination ?? new(new(), default);
        }
    }
}