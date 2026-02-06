namespace WeatherSite.Models.City.Requests;

public class GetCitiesByNameRequest
{
    public required string CityName { get; set; }
    public int Limit { get; set; }
}