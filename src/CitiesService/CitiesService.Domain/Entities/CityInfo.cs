using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CitiesService.Domain.Entities;

public class CityInfo
{
    [Key]
    public int Id { get; set; }


	/// <summary>
	/// This is a `city id` from json file
	/// </summary>
	[Required]
    public decimal CityId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? State { get; set; }

    [Required]
    public string CountryCode { get; set; } = null!;

	[Column(TypeName = "decimal(18, 2)")]
	public decimal Lon { get; set; }

	[Column(TypeName = "decimal(18, 2)")]
	public decimal Lat { get; set; }
}