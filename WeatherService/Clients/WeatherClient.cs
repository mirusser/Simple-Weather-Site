using System;
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
        public record Coord(decimal lon, decimal lat); //coordinates - longitude, latitude
        public record Weather(int id, string main, string description, string icon);
        public record Main(decimal temp, decimal feels_like, decimal temp_min, decimal temp_max, int pressure, int humidity);
        public record Wind(decimal speed, int deg);
        public record Clouds(int all);
        public record Sys(int type, int id, string country, long sunrise, long sunset);
        public record Forecast(Coord coord, Weather[] weather, Main main, int visibility, Wind wind, Clouds clouds, long dt, Sys sys, int timezone, int id, string name, int cod);
        #endregion

        public WeatherClient(
            HttpClient httpClient,
            IOptions<ServiceSettings> options)
        {
            _httpClient = httpClient;
            _serviceSettings = options.Value;
        }

        public async Task<Forecast> GetCurrentWeatherAsync(string city)
        {
            var forecast = await _httpClient.GetFromJsonAsync<Forecast>(
                $"https://{_serviceSettings.OpenWeatherHost}/data/2.5/weather?q={city}&appid={_serviceSettings.ApiKey}&units=metric"
            );

            return forecast;
        }

        public async Task<Forecast> GetCurrentWeatherMockAsync(string city)
        {
            Coord coord = new(Convert.ToDecimal(16.9299), Convert.ToDecimal(52.4069));
            Weather weather = new(801, "Clouds", "broken clouds", "02d");
            Main main = new (Convert.ToDecimal(2.01), Convert.ToDecimal(-3.32), Convert.ToDecimal(1.11), Convert.ToDecimal(2.78), 1024, 51);
            int visibility = 10000;
            Wind  wind = new(Convert.ToDecimal(3.6), 250);
            Clouds clouds = new(20);
            long dt = 1616253901;
            Sys sys = new(1, 1710, "PL", 1616216087, 1616259880);
            int timezone = 3600;
            int id = 3088171;
            string name = "Poznań";
            int cod = 200;
            
            var forecast = new Forecast(coord, new Weather[] { weather }, main, visibility, wind, clouds, dt, sys, timezone, id, name, cod);

            return forecast;
        }
    }
}