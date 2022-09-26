using System.ComponentModel.DataAnnotations;

namespace CitiesService.Domain.Entities;

public class CityInfo
{
    [Key]
    public int Id { get; set; }

    //This is a city id from json file
    [Required]
    public decimal CityId { get; set; }

    [Required]
    public string Name { get; set; }

    public string State { get; set; }

    [Required]
    public string CountryCode { get; set; }

    public decimal Lon { get; set; }

    public decimal Lat { get; set; }
}