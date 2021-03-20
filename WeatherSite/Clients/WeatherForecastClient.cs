using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WeatherSite.Clients
{
    public class WeatherForecastClient
    {
        #region Properties
        private readonly HttpClient _httpClient;
        #endregion

        #region records
        public record WeatherForecast(DateTime Date, int TemperatureC, int TemperatureF, string Summary);
        #endregion

        public WeatherForecastClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherForecast> GetCurrentWeatherForCity(string city = "Poznan")
        {
            WeatherForecast weatherForecast = null;
            try
            {
                weatherForecast = await _httpClient.GetFromJsonAsync<WeatherForecast>($"http://localhost:5050/api/weatherforecast/{city}");
            }
            catch (Exception ex)
            {
                //TODO: do something with error here
            }

            return weatherForecast;
        }
    }
}
