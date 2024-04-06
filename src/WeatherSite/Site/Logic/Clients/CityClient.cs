using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Options;
using WeatherSite.Clients.Models.Records;
using WeatherSite.Settings;

namespace WeatherSite.Clients;

//TODO: interface, HttpFactory, Polly, hardcoded strings, exception handling
public class CityClient
{
    #region Properties

    private readonly HttpClient _httpClient;
    private readonly ApiEndpoints _apiEndpoints;

    #endregion Properties

    public CityClient(
        HttpClient httpClient,
        IOptions<ApiEndpoints> options)
    {
        _httpClient = httpClient;
        _apiEndpoints = options.Value;
    }

    public async Task<List<City>> GetCitiesByName(string cityName, int limit = 10)
    {
        //string url = $"{_apiEndpoints.CitiesServiceApiUrl}/GetCitiesByName{HttpUtility.UrlEncode(cityName)}/{HttpUtility.UrlEncode(limit.ToString())}";

        string url = $"{_apiEndpoints.CitiesServiceApiUrl}GetCitiesByName";

        var getCitiesQuery = new
        {
            cityName,
            limit
        };

        var jsonContent = JsonContent.Create(getCitiesQuery);

        HttpResponseMessage response = await _httpClient.PostAsync(url, jsonContent);

        var content = await response.Content.ReadAsStringAsync();
        var cities = JsonConvert.DeserializeObject<CitiesResponse>(content);

        return cities?.Cities ?? [];
    }

    public async Task<CitiesPagination?> GetCitiesPagination(int pageNumber = 1, int numberOfCities = 25)
    {
        string url = $"{_apiEndpoints.CitiesServiceApiUrl}GetCitiesPagination/{HttpUtility.UrlEncode(numberOfCities.ToString())}/{HttpUtility.UrlEncode(pageNumber.ToString())}";

        CitiesPagination? citiesPagination = await _httpClient.GetFromJsonAsync<CitiesPagination>(url);

        return citiesPagination;
    }
}