using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WeatherService.Settings;

namespace WeatherService.Clients
{
    public class WeatherClient
    {
        #region Properties
        private readonly HttpClient _httpClient;
        private readonly ServiceSettings _serviceSettings;
        #endregion

        #region Records
        public record Weather(string description);
        public record Main(decimal temp);
        public record Forecast(Weather[] weather, Main main, long dt);
        #endregion

        public WeatherClient(
            HttpClient httpClient,
            IOptions<ServiceSettings> options)
        {
            _httpClient = httpClient;
            _serviceSettings = options.Value;
        }

        public async Task<Forecast> GetCurrentWeaterAsync(string city)
        {
            var forecast = await _httpClient.GetFromJsonAsync<Forecast>(
                $"https://{_serviceSettings.OpenWeatherHost}/data/2.5/weather?q={city}&appid={_serviceSettings.ApiKey}&units=metric"
            );

            return forecast;
        }
    }
}