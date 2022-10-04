using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WeatherSite.Clients.Models.Records;

namespace WeatherSite.Models.WeatherPrediction;

public class GetWeatherForecastVM
{
    public string CityName { get; set; }

    [Required]
    [DisplayName("City name")]
    public decimal CityId { get; set; }

    public string CitiesServiceEndpoint { get; set; }
    public string CitiesServiceLocalEndpoint { get; internal set; }

    //TODO: make a proper view model of this property maybe
    public WeatherForecast WeatherForecast { get; set; }
}