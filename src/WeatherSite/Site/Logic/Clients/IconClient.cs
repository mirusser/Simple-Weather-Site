using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WeatherSite.Clients.Models.Records;
using WeatherSite.Settings;

namespace WeatherSite.Logic.Clients
{
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
            string url = $"{_apiEndpoints.IconServiceApiUrl}GetIcon/{Uri.EscapeDataString(icon)}";
            IconDto? iconDto = await _httpClient.GetFromJsonAsync<IconDto>(url);

            return iconDto;
        }
    }
}