using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WeatherSite.Logic.Clients.Models.Records;
using WeatherSite.Logic.Settings;

namespace WeatherSite.Logic.Clients;

//TODO: interface, HttpFactory, Polly, hardcoded strings, exception handling
public class CityClient(
    HttpClient httpClient,
    IOptions<ApiEndpoints> options)
{
    private readonly ApiEndpoints apiEndpoints = options.Value; 

    public async Task<List<City>> GetCitiesByName(string cityName, int limit = 10)
    {
        //string url = $"{_apiEndpoints.CitiesServiceApiUrl}/GetCitiesByName{HttpUtility.UrlEncode(cityName)}/{HttpUtility.UrlEncode(limit.ToString())}";

        string url = $"{apiEndpoints.CitiesServiceApiUrl}GetCitiesByName";

        var getCitiesQuery = new
        {
            cityName,
            limit
        };

        var jsonContent = JsonContent.Create(getCitiesQuery);

        HttpResponseMessage response = await httpClient.PostAsync(url, jsonContent);

        var content = await response.Content.ReadAsStringAsync();
        var cities = JsonConvert.DeserializeObject<CitiesResponse>(content);

        return cities?.Cities ?? [];
    }

    public async Task<CitiesPagination?> GetCitiesPagination(int pageNumber = 1, int numberOfCities = 25)
    {
        string url = $"{apiEndpoints.CitiesServiceApiUrl}GetCitiesPagination/{HttpUtility.UrlEncode(numberOfCities.ToString())}/{HttpUtility.UrlEncode(pageNumber.ToString())}";

        CitiesPagination? citiesPagination = await httpClient.GetFromJsonAsync<CitiesPagination>(url);

        return citiesPagination;
    }
}